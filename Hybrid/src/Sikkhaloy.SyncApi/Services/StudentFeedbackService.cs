using Microsoft.Data.SqlClient;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.SyncApi.Services;

public sealed class StudentFeedbackService
{
    private readonly EduConnectionFactory _connections;

    public StudentFeedbackService(EduConnectionFactory connections) => _connections = connections;

    public async Task<List<StudentFeedbackRecipientDto>> ListRecipientsAsync(
        SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<StudentFeedbackRecipientDto>();
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            if (!IsStudent(session) && await ResolveOfficeAsync(con, session, ct) is null)
                return items;
            items = await LoadRecipientsAsync(con, session.SchoolID, teachers: false, ct);
        }
        catch (SqlException)
        {
        }

        return items;
    }

    public async Task<List<StudentFeedbackRecipientDto>> ListAssignRecipientsAsync(
        SessionSnapshot session, CancellationToken ct)
    {
        var items = new List<StudentFeedbackRecipientDto>();
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            var actor = await ResolveOfficeAsync(con, session, ct);
            if (actor is null || !actor.CanAssign)
                return items;
            items = await LoadRecipientsAsync(con, session.SchoolID, teachers: true, ct);
            if (actor.RegistrationID > 0)
                items = items.Where(x => x.RegistrationID != actor.RegistrationID).ToList();
        }
        catch (Exception)
        {
        }

        return items;
    }

    public async Task<List<StudentFeedbackItemDto>> ListMineAsync(SessionSnapshot session, CancellationToken ct)
    {
        if (!IsStudent(session))
            return [];
        return await QueryListAsync(session, """
SELECT f.FeedbackID, f.Kind, f.Title, f.Body, ISNULL(f.Status, N'Pending') AS Status,
       f.ToRegistrationID, f.ToName, f.ToDesignation, f.ToCategory, f.CreatedAt, 0 AS IsRead,
       ISNULL(f.AssignedByRegistrationID, 0) AS AssignedByRegistrationID,
       ISNULL(f.AssignedByName, N'') AS AssignedByName,
       ISNULL(f.AssignedByDesignation, N'') AS AssignedByDesignation,
       ISNULL(f.ProcessingByRegistrationID, 0) AS ProcessingByRegistrationID,
       ISNULL(f.ProcessingByName, N'') AS ProcessingByName,
       ISNULL(f.ProcessingByDesignation, N'') AS ProcessingByDesignation,
       f.ProcessingAt,
       ISNULL(f.SolvedByRegistrationID, 0) AS SolvedByRegistrationID,
       ISNULL(f.SolvedByName, N'') AS SolvedByName,
       ISNULL(f.SolvedByDesignation, N'') AS SolvedByDesignation,
       f.SolvedAt,
       ISNULL(st.StudentsName, N'') AS StudentName,
       ISNULL(st.ID, N'') AS StudentCode,
       ISNULL(cc.Class, N'') AS ClassName,
       ISNULL(cs.Section, N'') AS SectionName
FROM dbo.Student_Portal_Feedback AS f
LEFT JOIN dbo.Student AS st ON st.StudentID = f.StudentID
LEFT JOIN dbo.StudentsClass AS sc ON sc.StudentClassID = f.StudentClassID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.CreateSection AS cs ON cs.SectionID = sc.SectionID
WHERE f.SchoolID = @SchoolID AND f.StudentID = @StudentID
ORDER BY f.CreatedAt DESC, f.FeedbackID DESC
""", ct);
    }

    public async Task<StudentFeedbackResult> SubmitAsync(
        SessionSnapshot session, SubmitStudentFeedbackRequest? request, CancellationToken ct)
    {
        if (!IsStudent(session))
            return Fail("auth.forbidden");

        request ??= new SubmitStudentFeedbackRequest();
        var kind = NormalizeKind(request.Kind);
        if (kind is null)
            return Fail("fb.needKind");
        var title = (request.Title ?? "").Trim();
        var body = (request.Body ?? "").Trim();
        if (title.Length == 0)
            return Fail("fb.needTitle");
        if (body.Length == 0)
            return Fail("fb.needBody");
        if (title.Length > 200)
            title = title[..200];
        if (body.Length > 4000)
            body = body[..4000];
        if (request.ToRegistrationID <= 0)
            return Fail("fb.needTo");

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);

            var recipient = await LoadRecipientAsync(con, session.SchoolID, request.ToRegistrationID, ct);
            if (recipient is null)
                return Fail("fb.needTo");

            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Student_Portal_Feedback
    (SchoolID, EducationYearID, StudentID, StudentClassID, Kind, Title, Body, Status,
     ToRegistrationID, ToName, ToDesignation, ToCategory, CreatedAt)
OUTPUT INSERTED.FeedbackID
VALUES
    (@SchoolID, @EducationYearID, @StudentID, @StudentClassID, @Kind, @Title, @Body, N'Pending',
     @ToRegistrationID, @ToName, @ToDesignation, @ToCategory, GETDATE());
""", con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@EducationYearID", session.EducationYearID);
            cmd.Parameters.AddWithValue("@StudentID", session.StudentID);
            cmd.Parameters.AddWithValue("@StudentClassID", session.StudentClassID);
            cmd.Parameters.AddWithValue("@Kind", kind);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Body", body);
            cmd.Parameters.AddWithValue("@ToRegistrationID", recipient.RegistrationID);
            cmd.Parameters.AddWithValue("@ToName", recipient.Name);
            cmd.Parameters.AddWithValue("@ToDesignation", recipient.Designation);
            cmd.Parameters.AddWithValue("@ToCategory", recipient.Category);
            var id = ToInt(await cmd.ExecuteScalarAsync(ct));
            return new StudentFeedbackResult { Succeeded = true, FeedbackID = id };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    public async Task<List<StudentFeedbackItemDto>> ListOfficeAsync(SessionSnapshot session, CancellationToken ct)
    {
        await using var con = _connections.Create();
        await con.OpenAsync(ct);
        await EnsureTablesAsync(con, ct);

        var actor = await ResolveOfficeAsync(con, session, ct);
        var roleAdmin = string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var seeAll = roleAdmin || (actor?.IsMainAdmin ?? false);
        if (session.SchoolID <= 0)
            return [];
        if (actor is null && !seeAll)
            return [];

        var viewerId = actor?.RegistrationID > 0 ? actor.RegistrationID : Math.Max(session.RegistrationID, 0);

        try
        {
            var items = await QueryListAsync(con, ListSql(seeAll), session, viewerId, ct);
            await AttachRepliesAsync(con, session.SchoolID, viewerId, items, ct);
            return items;
        }
        catch (Exception)
        {
            var items = await QueryListAsync(con, ListSqlSimple(seeAll), session, viewerId, ct);
            await AttachRepliesAsync(con, session.SchoolID, viewerId, items, ct);
            return items;
        }
    }

    public async Task<StudentFeedbackUnreadDto> UnreadAsync(SessionSnapshot session, CancellationToken ct)
    {
        var dto = new StudentFeedbackUnreadDto();
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);
            var actor = await ResolveOfficeAsync(con, session, ct);
            var roleAdmin = string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (actor is null)
            {
                if (roleAdmin && session.SchoolID > 0)
                {
                    dto.IsMainAdmin = true;
                    await using var countCmd = new SqlCommand("""
SELECT COUNT(*)
FROM dbo.Student_Portal_Feedback AS f
WHERE f.SchoolID = @SchoolID
  AND ISNULL(f.Status, N'Pending') = N'Pending'
""", con) { CommandTimeout = 20 };
                    countCmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                    dto.Count = ToInt(await countCmd.ExecuteScalarAsync(ct));
                }
                return dto;
            }
            dto.IsMainAdmin = actor.IsMainAdmin || roleAdmin;
            var seeAll = dto.IsMainAdmin;
            // Main admin badge = Pending workflow count.
            // Teacher / Sub-Admin badge = items assigned to them that THEY have not opened (per RegistrationID read).
            string sql;
            if (seeAll)
            {
                sql = """
SELECT COUNT(*)
FROM dbo.Student_Portal_Feedback AS f
WHERE f.SchoolID = @SchoolID
  AND ISNULL(f.Status, N'Pending') = N'Pending'
""";
            }
            else
            {
                sql = """
SELECT COUNT(*)
FROM dbo.Student_Portal_Feedback AS f
LEFT JOIN dbo.Student_Portal_Feedback_Read AS rd
    ON rd.FeedbackID = f.FeedbackID AND rd.RegistrationID = @ViewerID
WHERE f.SchoolID = @SchoolID
  AND f.ToRegistrationID = @ViewerID
  AND ISNULL(f.Status, N'Pending') <> N'Solved'
  AND rd.FeedbackID IS NULL
""";
            }
            await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@ViewerID", actor.RegistrationID);
            dto.Count = ToInt(await cmd.ExecuteScalarAsync(ct));
        }
        catch (Exception)
        {
        }

        return dto;
    }

    public async Task<StudentFeedbackResult> SetStatusAsync(
        SessionSnapshot session, int feedbackId, string? status, CancellationToken ct)
    {
        var next = NormalizeStatus(status);
        if (next is null || feedbackId <= 0)
            return Fail("fb.needStatus");

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);
            var actor = await ResolveOfficeAsync(con, session, ct);
            if (actor is null || !await CanViewAsync(con, session.SchoolID, actor, feedbackId, ct))
                return Fail("auth.forbidden");

            var current = await ReadStatusAsync(con, session.SchoolID, feedbackId, ct);
            if (string.IsNullOrWhiteSpace(current))
                return Fail("auth.forbidden");
            if (!CanMoveStatus(current, next, actor.IsMainAdmin))
                return Fail("fb.statusBlocked");

            var by = await ResolveActorDisplayAsync(con, session, actor, ct);
            if (next == "Processing")
            {
                await using var cmd = new SqlCommand("""
UPDATE dbo.Student_Portal_Feedback
SET Status = @Status,
    ProcessingByRegistrationID = @ById,
    ProcessingByName = @ByName,
    ProcessingByDesignation = @ByDesig,
    ProcessingAt = GETDATE()
WHERE FeedbackID = @FeedbackID AND SchoolID = @SchoolID
""", con) { CommandTimeout = 20 };
                cmd.Parameters.AddWithValue("@Status", next);
                cmd.Parameters.AddWithValue("@ById", by.RegistrationID);
                cmd.Parameters.AddWithValue("@ByName", by.Name);
                cmd.Parameters.AddWithValue("@ByDesig", by.Designation);
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await cmd.ExecuteNonQueryAsync(ct);
            }
            else
            {
                await using var cmd = new SqlCommand("""
UPDATE dbo.Student_Portal_Feedback
SET Status = @Status,
    SolvedByRegistrationID = @ById,
    SolvedByName = @ByName,
    SolvedByDesignation = @ByDesig,
    SolvedAt = GETDATE()
WHERE FeedbackID = @FeedbackID AND SchoolID = @SchoolID
""", con) { CommandTimeout = 20 };
                cmd.Parameters.AddWithValue("@Status", next);
                cmd.Parameters.AddWithValue("@ById", by.RegistrationID);
                cmd.Parameters.AddWithValue("@ByName", by.Name);
                cmd.Parameters.AddWithValue("@ByDesig", by.Designation);
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            if (next is "Processing" or "Solved")
                await MarkReadRowAsync(con, feedbackId, actor.RegistrationID, ct);
            return new StudentFeedbackResult { Succeeded = true, FeedbackID = feedbackId };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    public async Task<StudentFeedbackResult> AssignAsync(
        SessionSnapshot session, int feedbackId, int toRegistrationId, CancellationToken ct)
    {
        if (feedbackId <= 0 || toRegistrationId <= 0)
            return Fail("fb.needTo");

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);
            var actor = await ResolveOfficeAsync(con, session, ct);
            if (actor is null || !actor.CanAssign || !await CanViewAsync(con, session.SchoolID, actor, feedbackId, ct))
                return Fail("auth.forbidden");

            var recipient = await LoadAssignRecipientAsync(con, session.SchoolID, toRegistrationId, ct);
            if (recipient is null)
                return Fail("fb.needTo");

            var assigner = await LoadAssignRecipientAsync(con, session.SchoolID, actor.RegistrationID, ct);
            var assignerName = assigner?.Name?.Trim() ?? "";
            if (assignerName.Length == 0)
                assignerName = (session.DisplayName ?? session.UserName ?? "").Trim();
            var assignerDesig = assigner?.Designation?.Trim() ?? "";

            await using var cmd = new SqlCommand("""
UPDATE dbo.Student_Portal_Feedback
SET ToRegistrationID = @ToRegistrationID,
    ToName = @ToName,
    ToDesignation = @ToDesignation,
    ToCategory = @ToCategory,
    AssignedByRegistrationID = @AssignedByRegistrationID,
    AssignedByName = @AssignedByName,
    AssignedByDesignation = @AssignedByDesignation
WHERE FeedbackID = @FeedbackID AND SchoolID = @SchoolID
""", con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@ToRegistrationID", recipient.RegistrationID);
            cmd.Parameters.AddWithValue("@ToName", recipient.Name);
            cmd.Parameters.AddWithValue("@ToDesignation", recipient.Designation);
            cmd.Parameters.AddWithValue("@ToCategory", recipient.Category);
            cmd.Parameters.AddWithValue("@AssignedByRegistrationID", actor.RegistrationID);
            cmd.Parameters.AddWithValue("@AssignedByName", assignerName.Length == 0 ? "—" : assignerName);
            cmd.Parameters.AddWithValue("@AssignedByDesignation", assignerDesig);
            cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            await cmd.ExecuteNonQueryAsync(ct);

            // New assignee starts unread for their own RegistrationID (admin read must not count).
            await using var clearCmd = new SqlCommand("""
DELETE FROM dbo.Student_Portal_Feedback_Read
WHERE FeedbackID = @FeedbackID AND RegistrationID = @ToRegistrationID
""", con) { CommandTimeout = 20 };
            clearCmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
            clearCmd.Parameters.AddWithValue("@ToRegistrationID", recipient.RegistrationID);
            await clearCmd.ExecuteNonQueryAsync(ct);

            return new StudentFeedbackResult { Succeeded = true, FeedbackID = feedbackId };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    public async Task<StudentFeedbackResult> MarkReadAsync(
        SessionSnapshot session, int feedbackId, CancellationToken ct)
    {
        if (feedbackId <= 0)
            return Fail("auth.forbidden");
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);
            var actor = await ResolveOfficeAsync(con, session, ct);
            if (actor is null || !await CanViewAsync(con, session.SchoolID, actor, feedbackId, ct))
                return Fail("auth.forbidden");
            await MarkReadRowAsync(con, feedbackId, actor.RegistrationID, ct);
            return new StudentFeedbackResult { Succeeded = true, FeedbackID = feedbackId };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    public async Task<StudentFeedbackResult> MarkAllReadAsync(SessionSnapshot session, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);
            var actor = await ResolveOfficeAsync(con, session, ct);
            if (actor is null)
                return Fail("auth.forbidden");
            var sql = """
INSERT INTO dbo.Student_Portal_Feedback_Read (FeedbackID, RegistrationID, ReadAt)
SELECT f.FeedbackID, @ViewerID, GETDATE()
FROM dbo.Student_Portal_Feedback AS f
LEFT JOIN dbo.Student_Portal_Feedback_Read AS rd
    ON rd.FeedbackID = f.FeedbackID AND rd.RegistrationID = @ViewerID
WHERE f.SchoolID = @SchoolID
  AND rd.FeedbackID IS NULL
""" + (actor.IsMainAdmin ? "" : "  AND f.ToRegistrationID = @ViewerID\n");
            await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@ViewerID", actor.RegistrationID);
            await cmd.ExecuteNonQueryAsync(ct);
            return new StudentFeedbackResult { Succeeded = true };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    public async Task<StudentFeedbackResult> ReplyAsync(
        SessionSnapshot session, int feedbackId, string? body, CancellationToken ct)
    {
        var text = (body ?? "").Trim();
        if (feedbackId <= 0)
            return Fail("auth.forbidden");
        if (text.Length == 0)
            return Fail("fb.needReply");
        if (text.Length > 4000)
            text = text[..4000];

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);

            string fromName;
            string fromDesig;
            string fromCategory;
            int fromId;

            if (IsStudent(session))
            {
                if (!await CanStudentOwnAsync(con, session, feedbackId, ct))
                    return Fail("auth.forbidden");
                fromId = session.RegistrationID > 0 ? session.RegistrationID : session.StudentID;
                fromName = (session.DisplayName ?? "").Trim();
                if (fromName.Length == 0)
                    fromName = session.UserName ?? "Student";
                fromDesig = "";
                fromCategory = "Student";
            }
            else
            {
                var actor = await ResolveOfficeAsync(con, session, ct);
                if (actor is null || !await CanViewAsync(con, session.SchoolID, actor, feedbackId, ct))
                    return Fail("auth.forbidden");
                var by = await ResolveActorDisplayAsync(con, session, actor, ct);
                fromId = by.RegistrationID;
                fromName = by.Name;
                fromDesig = by.Designation;
                fromCategory = actor.Category;
            }

            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Student_Portal_Feedback_Reply
    (FeedbackID, SchoolID, FromRegistrationID, FromName, FromDesignation, FromCategory, Body, CreatedAt)
OUTPUT INSERTED.ReplyID
VALUES
    (@FeedbackID, @SchoolID, @FromRegistrationID, @FromName, @FromDesignation, @FromCategory, @Body, GETDATE());
""", con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@FromRegistrationID", fromId);
            cmd.Parameters.AddWithValue("@FromName", fromName);
            cmd.Parameters.AddWithValue("@FromDesignation", fromDesig);
            cmd.Parameters.AddWithValue("@FromCategory", fromCategory);
            cmd.Parameters.AddWithValue("@Body", text);
            var replyId = ToInt(await cmd.ExecuteScalarAsync(ct));
            return new StudentFeedbackResult { Succeeded = true, FeedbackID = feedbackId, ReplyID = replyId };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    public async Task<StudentFeedbackResult> MarkRepliesReadAsync(
        SessionSnapshot session, int feedbackId, CancellationToken ct)
    {
        if (feedbackId <= 0)
            return Fail("auth.forbidden");

        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);

            int viewerId;
            if (IsStudent(session))
            {
                if (!await CanStudentOwnAsync(con, session, feedbackId, ct))
                    return Fail("auth.forbidden");
                viewerId = ResolveViewerKey(session);
            }
            else
            {
                var actor = await ResolveOfficeAsync(con, session, ct);
                if (actor is null || !await CanViewAsync(con, session.SchoolID, actor, feedbackId, ct))
                    return Fail("auth.forbidden");
                viewerId = actor.RegistrationID;
            }

            if (viewerId <= 0)
                return Fail("auth.forbidden");

            await using var cmd = new SqlCommand("""
INSERT INTO dbo.Student_Portal_Feedback_Reply_Read (ReplyID, RegistrationID, ReadAt)
SELECT r.ReplyID, @ViewerID, GETDATE()
FROM dbo.Student_Portal_Feedback_Reply AS r
LEFT JOIN dbo.Student_Portal_Feedback_Reply_Read AS rd
    ON rd.ReplyID = r.ReplyID AND rd.RegistrationID = @ViewerID
WHERE r.FeedbackID = @FeedbackID
  AND r.SchoolID = @SchoolID
  AND rd.ReplyID IS NULL
""", con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
            cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
            cmd.Parameters.AddWithValue("@ViewerID", viewerId);
            await cmd.ExecuteNonQueryAsync(ct);
            return new StudentFeedbackResult { Succeeded = true, FeedbackID = feedbackId };
        }
        catch (SqlException)
        {
            return Fail("fb.failed");
        }
    }

    private static string ListSql(bool seeAll)
    {
        var toFilter = seeAll ? "" : "\n  AND f.ToRegistrationID = @ViewerID";
        return $"""
SELECT f.FeedbackID, f.Kind, f.Title, f.Body, ISNULL(f.Status, N'Pending') AS Status,
       f.ToRegistrationID, f.ToName, f.ToDesignation, f.ToCategory, f.CreatedAt,
       ISNULL(f.AssignedByRegistrationID, 0) AS AssignedByRegistrationID,
       ISNULL(f.AssignedByName, N'') AS AssignedByName,
       ISNULL(f.AssignedByDesignation, N'') AS AssignedByDesignation,
       ISNULL(f.ProcessingByRegistrationID, 0) AS ProcessingByRegistrationID,
       ISNULL(f.ProcessingByName, N'') AS ProcessingByName,
       ISNULL(f.ProcessingByDesignation, N'') AS ProcessingByDesignation,
       f.ProcessingAt,
       ISNULL(f.SolvedByRegistrationID, 0) AS SolvedByRegistrationID,
       ISNULL(f.SolvedByName, N'') AS SolvedByName,
       ISNULL(f.SolvedByDesignation, N'') AS SolvedByDesignation,
       f.SolvedAt,
       CAST(CASE WHEN rd.FeedbackID IS NULL THEN 0 ELSE 1 END AS int) AS IsRead,
       ISNULL(st.StudentsName, N'') AS StudentName,
       ISNULL(st.ID, N'') AS StudentCode,
       ISNULL(cc.Class, N'') AS ClassName,
       ISNULL(cs.Section, N'') AS SectionName
FROM dbo.Student_Portal_Feedback AS f
LEFT JOIN dbo.Student AS st ON st.StudentID = f.StudentID
LEFT JOIN dbo.StudentsClass AS sc ON sc.StudentClassID = f.StudentClassID
LEFT JOIN dbo.CreateClass AS cc ON cc.ClassID = sc.ClassID
LEFT JOIN dbo.CreateSection AS cs ON cs.SectionID = sc.SectionID
LEFT JOIN dbo.Student_Portal_Feedback_Read AS rd
    ON rd.FeedbackID = f.FeedbackID AND rd.RegistrationID = @ViewerID
WHERE f.SchoolID = @SchoolID{toFilter}
ORDER BY CASE ISNULL(f.Status, N'Pending') WHEN N'Pending' THEN 0 WHEN N'Processing' THEN 1 ELSE 2 END,
         f.CreatedAt DESC, f.FeedbackID DESC
""";
    }

    private static string ListSqlSimple(bool seeAll)
    {
        var toFilter = seeAll ? "" : "\n  AND f.ToRegistrationID = @ViewerID";
        return $"""
SELECT f.FeedbackID, f.Kind, f.Title, f.Body, ISNULL(f.Status, N'Pending') AS Status,
       f.ToRegistrationID, f.ToName, f.ToDesignation, f.ToCategory, f.CreatedAt,
       ISNULL(f.AssignedByRegistrationID, 0) AS AssignedByRegistrationID,
       ISNULL(f.AssignedByName, N'') AS AssignedByName,
       ISNULL(f.AssignedByDesignation, N'') AS AssignedByDesignation,
       ISNULL(f.ProcessingByRegistrationID, 0) AS ProcessingByRegistrationID,
       ISNULL(f.ProcessingByName, N'') AS ProcessingByName,
       ISNULL(f.ProcessingByDesignation, N'') AS ProcessingByDesignation,
       f.ProcessingAt,
       ISNULL(f.SolvedByRegistrationID, 0) AS SolvedByRegistrationID,
       ISNULL(f.SolvedByName, N'') AS SolvedByName,
       ISNULL(f.SolvedByDesignation, N'') AS SolvedByDesignation,
       f.SolvedAt,
       0 AS IsRead,
       N'' AS StudentName,
       N'' AS StudentCode,
       N'' AS ClassName,
       N'' AS SectionName
FROM dbo.Student_Portal_Feedback AS f
WHERE f.SchoolID = @SchoolID{toFilter}
ORDER BY CASE ISNULL(f.Status, N'Pending') WHEN N'Pending' THEN 0 WHEN N'Processing' THEN 1 ELSE 2 END,
         f.CreatedAt DESC, f.FeedbackID DESC
""";
    }

    private async Task<List<StudentFeedbackItemDto>> QueryListAsync(
        SessionSnapshot session, string sql, CancellationToken ct)
    {
        try
        {
            await using var con = _connections.Create();
            await con.OpenAsync(ct);
            await EnsureTablesAsync(con, ct);
            var items = await QueryListAsync(con, sql, session, session.RegistrationID, ct);
            var viewerId = ResolveViewerKey(session);
            await AttachRepliesAsync(con, session.SchoolID, viewerId, items, ct);
            return items;
        }
        catch (SqlException)
        {
            return [];
        }
    }

    private static async Task<List<StudentFeedbackItemDto>> QueryListAsync(
        SqlConnection con, string sql, SessionSnapshot session, int viewerId, CancellationToken ct)
    {
        var items = new List<StudentFeedbackItemDto>();
        await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 20 };
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@StudentID", session.StudentID);
        cmd.Parameters.AddWithValue("@ViewerID", viewerId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new StudentFeedbackItemDto
            {
                FeedbackID = ToInt(reader["FeedbackID"]),
                Kind = Text(reader["Kind"]),
                Title = Text(reader["Title"]),
                Body = Text(reader["Body"]),
                Status = NormalizeStatus(Text(reader["Status"])) ?? "Pending",
                ToRegistrationID = ToInt(reader["ToRegistrationID"]),
                ToName = Text(reader["ToName"]),
                ToDesignation = Text(reader["ToDesignation"]),
                ToCategory = Text(reader["ToCategory"]),
                AssignedByRegistrationID = ReadColInt(reader, "AssignedByRegistrationID"),
                AssignedByName = ReadColText(reader, "AssignedByName"),
                AssignedByDesignation = ReadColText(reader, "AssignedByDesignation"),
                ProcessingByRegistrationID = ReadColInt(reader, "ProcessingByRegistrationID"),
                ProcessingByName = ReadColText(reader, "ProcessingByName"),
                ProcessingByDesignation = ReadColText(reader, "ProcessingByDesignation"),
                ProcessingAt = ReadColDate(reader, "ProcessingAt"),
                SolvedByRegistrationID = ReadColInt(reader, "SolvedByRegistrationID"),
                SolvedByName = ReadColText(reader, "SolvedByName"),
                SolvedByDesignation = ReadColText(reader, "SolvedByDesignation"),
                SolvedAt = ReadColDate(reader, "SolvedAt"),
                CreatedAt = reader["CreatedAt"] is DateTime d ? d : DateTime.MinValue,
                IsRead = ToInt(reader["IsRead"]) == 1,
                StudentName = Text(reader["StudentName"]),
                StudentCode = Text(reader["StudentCode"]),
                ClassName = Text(reader["ClassName"]),
                SectionName = Text(reader["SectionName"])
            });
        }

        return items;
    }

    private static async Task<OfficeActor?> ResolveOfficeAsync(
        SqlConnection con, SessionSnapshot session, CancellationToken ct)
    {
        if (session.SchoolID <= 0)
            return null;

        OfficeActor? actor = null;
        if (session.RegistrationID > 0)
        {
            actor = await ReadActorAsync(con, """
SELECT TOP 1 RegistrationID, Category
FROM dbo.Registration
WHERE SchoolID = @SchoolID AND RegistrationID = @RegistrationID
  AND Validation = N'Valid' AND Category IN (N'Admin', N'Sub-Admin', N'Teacher')
""", session.SchoolID, session.RegistrationID, session.UserName, ct);
        }

        if (actor is null && !string.IsNullOrWhiteSpace(session.UserName))
        {
            actor = await ReadActorAsync(con, """
SELECT TOP 1 RegistrationID, Category
FROM dbo.Registration
WHERE SchoolID = @SchoolID AND UserName = @UserName
  AND Validation = N'Valid' AND Category IN (N'Admin', N'Sub-Admin', N'Teacher')
""", session.SchoolID, session.RegistrationID, session.UserName, ct);
        }

        var schoolAdmin = await ReadSchoolAdminAsync(con, session.SchoolID, ct);
        var jwtAdmin = string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var schoolAdminUser = schoolAdmin is not null
            && !string.IsNullOrWhiteSpace(session.UserName)
            && string.Equals(schoolAdmin.UserName, session.UserName, StringComparison.OrdinalIgnoreCase);
        var schoolAdminId = schoolAdmin is not null
            && session.RegistrationID > 0
            && schoolAdmin.RegistrationID == session.RegistrationID;
        var isMainAdmin = jwtAdmin
            || schoolAdminUser
            || schoolAdminId
            || (actor is not null && actor.IsMainAdmin);

        if (isMainAdmin)
        {
            return new OfficeActor
            {
                RegistrationID = actor?.RegistrationID
                    ?? schoolAdmin?.RegistrationID
                    ?? session.RegistrationID,
                Category = "Admin"
            };
        }

        if (actor is not null)
            return actor;

        if (IsOfficeRole(session) && session.RegistrationID > 0)
        {
            return new OfficeActor
            {
                RegistrationID = session.RegistrationID,
                Category = "Sub-Admin"
            };
        }

        if (session.IsTeacher && session.RegistrationID > 0)
        {
            return new OfficeActor
            {
                RegistrationID = session.RegistrationID,
                Category = "Teacher"
            };
        }

        return null;
    }

    private static async Task<SchoolAdminRow?> ReadSchoolAdminAsync(
        SqlConnection con, int schoolId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT TOP 1 r.RegistrationID, r.UserName
FROM dbo.SchoolInfo AS s
INNER JOIN dbo.Registration AS r
    ON r.SchoolID = s.SchoolID AND r.UserName = s.UserName
WHERE s.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
""", con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return new SchoolAdminRow
        {
            RegistrationID = ToInt(reader["RegistrationID"]),
            UserName = Text(reader["UserName"])
        };
    }

    private static async Task<OfficeActor?> ReadActorAsync(
        SqlConnection con, string sql, int schoolId, int registrationId, string userName, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        cmd.Parameters.AddWithValue("@RegistrationID", registrationId);
        cmd.Parameters.AddWithValue("@UserName", userName ?? "");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return new OfficeActor
        {
            RegistrationID = ToInt(reader["RegistrationID"]),
            Category = Text(reader["Category"])
        };
    }

    private static async Task<List<StudentFeedbackRecipientDto>> LoadRecipientsAsync(
        SqlConnection con, int schoolId, bool teachers, CancellationToken ct)
    {
        var items = new List<StudentFeedbackRecipientDto>();
        await using (var cmd = new SqlCommand("""
SELECT r.RegistrationID,
       r.Category,
       LTRIM(RTRIM(ISNULL(a.FirstName, N'') + N' ' + ISNULL(a.LastName, N''))) AS Name,
       ISNULL(a.Designation, N'') AS Designation
FROM dbo.Admin AS a
INNER JOIN dbo.Registration AS r ON a.RegistrationID = r.RegistrationID
WHERE a.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
  AND r.Category IN (N'Admin', N'Sub-Admin')
ORDER BY CASE WHEN r.Category = N'Sub-Admin' THEN 0 ELSE 1 END,
         Name, r.RegistrationID
""", con) { CommandTimeout = 20 })
        {
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                AddRecipient(items, reader);
        }

        if (!teachers)
            return items;

        await using (var cmd = new SqlCommand("""
SELECT r.RegistrationID,
       N'Teacher' AS Category,
       LTRIM(RTRIM(ISNULL(t.FirstName, N'') + N' ' + ISNULL(t.LastName, N''))) AS Name,
       ISNULL(t.Designation, N'') AS Designation
FROM dbo.Teacher AS t
INNER JOIN dbo.Registration AS r
    ON r.SchoolID = t.SchoolID
   AND (t.TeacherRegistrationID = r.RegistrationID OR t.RegistrationID = r.RegistrationID)
WHERE t.SchoolID = @SchoolID
  AND r.Validation = N'Valid'
  AND r.Category = N'Teacher'
ORDER BY Name, r.RegistrationID
""", con) { CommandTimeout = 20 })
        {
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                AddRecipient(items, reader);
        }

        return items;
    }

    private static void AddRecipient(List<StudentFeedbackRecipientDto> items, SqlDataReader reader)
    {
        var name = Text(reader["Name"]);
        if (string.IsNullOrWhiteSpace(name))
            return;
        var id = ToInt(reader["RegistrationID"]);
        if (id <= 0 || items.Any(x => x.RegistrationID == id))
            return;
        items.Add(new StudentFeedbackRecipientDto
        {
            RegistrationID = id,
            Category = Text(reader["Category"]),
            Name = name,
            Designation = Text(reader["Designation"])
        });
    }

    private static async Task<StudentFeedbackRecipientDto?> LoadAssignRecipientAsync(
        SqlConnection con, int schoolId, int registrationId, CancellationToken ct)
    {
        var rows = await LoadRecipientsAsync(con, schoolId, teachers: true, ct);
        return rows.FirstOrDefault(x => x.RegistrationID == registrationId);
    }

    private static async Task<StudentFeedbackRecipientDto?> LoadRecipientAsync(
        SqlConnection con, int schoolId, int registrationId, CancellationToken ct)
    {
        var rows = await LoadRecipientsAsync(con, schoolId, teachers: false, ct);
        return rows.FirstOrDefault(x => x.RegistrationID == registrationId);
    }

    private static async Task<bool> CanViewAsync(
        SqlConnection con, int schoolId, OfficeActor actor, int feedbackId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT ToRegistrationID
FROM dbo.Student_Portal_Feedback
WHERE FeedbackID = @FeedbackID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        var toId = ToInt(await cmd.ExecuteScalarAsync(ct));
        if (toId <= 0)
            return false;
        return actor.IsMainAdmin || toId == actor.RegistrationID;
    }

    private static async Task<string> ReadStatusAsync(
        SqlConnection con, int schoolId, int feedbackId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT ISNULL(Status, N'Pending')
FROM dbo.Student_Portal_Feedback
WHERE FeedbackID = @FeedbackID AND SchoolID = @SchoolID
""", con);
        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
        cmd.Parameters.AddWithValue("@SchoolID", schoolId);
        return NormalizeStatus(Text(await cmd.ExecuteScalarAsync(ct))) ?? "";
    }

    private static async Task AttachRepliesAsync(
        SqlConnection con, int schoolId, int viewerId, List<StudentFeedbackItemDto> items, CancellationToken ct)
    {
        if (items.Count == 0)
            return;
        try
        {
            var map = items.ToDictionary(x => x.FeedbackID);
            await using var cmd = new SqlCommand("""
SELECT r.ReplyID, r.FeedbackID, r.FromRegistrationID, r.FromName, r.FromDesignation, r.FromCategory, r.Body, r.CreatedAt,
       CAST(CASE WHEN rd.ReplyID IS NULL THEN 0 ELSE 1 END AS int) AS IsRead
FROM dbo.Student_Portal_Feedback_Reply AS r
LEFT JOIN dbo.Student_Portal_Feedback_Reply_Read AS rd
    ON rd.ReplyID = r.ReplyID AND rd.RegistrationID = @ViewerID
WHERE r.SchoolID = @SchoolID
ORDER BY r.CreatedAt ASC, r.ReplyID ASC
""", con) { CommandTimeout = 20 };
            cmd.Parameters.AddWithValue("@SchoolID", schoolId);
            cmd.Parameters.AddWithValue("@ViewerID", viewerId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var fbId = ToInt(reader["FeedbackID"]);
                if (!map.TryGetValue(fbId, out var item))
                    continue;
                var fromId = ToInt(reader["FromRegistrationID"]);
                var isRead = ToInt(reader["IsRead"]) == 1 || (viewerId > 0 && fromId == viewerId);
                item.Replies.Add(new StudentFeedbackReplyDto
                {
                    ReplyID = ToInt(reader["ReplyID"]),
                    FeedbackID = fbId,
                    FromRegistrationID = fromId,
                    FromName = Text(reader["FromName"]),
                    FromDesignation = Text(reader["FromDesignation"]),
                    FromCategory = Text(reader["FromCategory"]),
                    Body = Text(reader["Body"]),
                    CreatedAt = reader["CreatedAt"] is DateTime d ? d : DateTime.MinValue
                });
                item.ReplyCount++;
                if (!isRead)
                    item.UnreadReplyCount++;
            }
        }
        catch (SqlException)
        {
            // Reply table may be missing until EnsureTables; counts stay 0.
        }
    }

    private static int ResolveViewerKey(SessionSnapshot session)
    {
        if (session.RegistrationID > 0)
            return session.RegistrationID;
        if (IsStudent(session) && session.StudentID > 0)
            return session.StudentID;
        return 0;
    }

    private static async Task<StudentFeedbackRecipientDto> ResolveActorDisplayAsync(
        SqlConnection con, SessionSnapshot session, OfficeActor actor, CancellationToken ct)
    {
        var person = await LoadAssignRecipientAsync(con, session.SchoolID, actor.RegistrationID, ct)
                     ?? await LoadRecipientAsync(con, session.SchoolID, actor.RegistrationID, ct);
        var name = person?.Name?.Trim() ?? "";
        if (name.Length == 0)
            name = (session.DisplayName ?? session.UserName ?? "").Trim();
        if (name.Length == 0)
            name = "—";
        return new StudentFeedbackRecipientDto
        {
            RegistrationID = actor.RegistrationID,
            Name = name,
            Designation = person?.Designation?.Trim() ?? "",
            Category = actor.Category
        };
    }

    private static async Task<bool> CanStudentOwnAsync(
        SqlConnection con, SessionSnapshot session, int feedbackId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
SELECT COUNT(1)
FROM dbo.Student_Portal_Feedback
WHERE FeedbackID = @FeedbackID AND SchoolID = @SchoolID AND StudentID = @StudentID
""", con);
        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
        cmd.Parameters.AddWithValue("@SchoolID", session.SchoolID);
        cmd.Parameters.AddWithValue("@StudentID", session.StudentID);
        return ToInt(await cmd.ExecuteScalarAsync(ct)) > 0;
    }

    private static async Task MarkReadRowAsync(
        SqlConnection con, int feedbackId, int registrationId, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF NOT EXISTS (
    SELECT 1 FROM dbo.Student_Portal_Feedback_Read
    WHERE FeedbackID = @FeedbackID AND RegistrationID = @ViewerID)
    INSERT INTO dbo.Student_Portal_Feedback_Read (FeedbackID, RegistrationID, ReadAt)
    VALUES (@FeedbackID, @ViewerID, GETDATE());
""", con);
        cmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
        cmd.Parameters.AddWithValue("@ViewerID", registrationId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task EnsureTablesAsync(SqlConnection con, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("""
IF OBJECT_ID(N'dbo.Student_Portal_Feedback', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback (
        FeedbackID int IDENTITY(1,1) NOT NULL,
        SchoolID int NOT NULL,
        EducationYearID int NOT NULL,
        StudentID int NOT NULL,
        StudentClassID int NOT NULL,
        Kind nvarchar(20) NOT NULL,
        Title nvarchar(200) NOT NULL,
        Body nvarchar(max) NOT NULL,
        Status nvarchar(20) NOT NULL CONSTRAINT DF_StudentPortalFeedback_Status DEFAULT (N'Pending'),
        ToRegistrationID int NOT NULL,
        ToName nvarchar(200) NOT NULL,
        ToDesignation nvarchar(200) NOT NULL CONSTRAINT DF_StudentPortalFeedback_Desig DEFAULT (N''),
        ToCategory nvarchar(20) NOT NULL,
        AssignedByRegistrationID int NULL,
        AssignedByName nvarchar(200) NULL,
        AssignedByDesignation nvarchar(200) NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedback_Created DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback PRIMARY KEY (FeedbackID)
    );
    CREATE INDEX IX_Student_Portal_Feedback_School ON dbo.Student_Portal_Feedback (SchoolID, CreatedAt DESC);
    CREATE INDEX IX_Student_Portal_Feedback_Student ON dbo.Student_Portal_Feedback (SchoolID, StudentID, CreatedAt DESC);
    CREATE INDEX IX_Student_Portal_Feedback_To ON dbo.Student_Portal_Feedback (SchoolID, ToRegistrationID, CreatedAt DESC);
END
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'Status') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD Status nvarchar(20) NOT NULL CONSTRAINT DF_StudentPortalFeedback_Status DEFAULT (N'Pending');
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'AssignedByRegistrationID') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD AssignedByRegistrationID int NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'AssignedByName') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD AssignedByName nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'AssignedByDesignation') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD AssignedByDesignation nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'ProcessingByRegistrationID') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD ProcessingByRegistrationID int NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'ProcessingByName') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD ProcessingByName nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'ProcessingByDesignation') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD ProcessingByDesignation nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'ProcessingAt') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD ProcessingAt datetime NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'SolvedByRegistrationID') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD SolvedByRegistrationID int NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'SolvedByName') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD SolvedByName nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'SolvedByDesignation') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD SolvedByDesignation nvarchar(200) NULL;
IF COL_LENGTH(N'dbo.Student_Portal_Feedback', N'SolvedAt') IS NULL
    ALTER TABLE dbo.Student_Portal_Feedback
    ADD SolvedAt datetime NULL;
IF OBJECT_ID(N'dbo.Student_Portal_Feedback_Read', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback_Read (
        FeedbackID int NOT NULL,
        RegistrationID int NOT NULL,
        ReadAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedbackRead_At DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback_Read PRIMARY KEY (FeedbackID, RegistrationID)
    );
END
IF OBJECT_ID(N'dbo.Student_Portal_Feedback_Reply', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback_Reply (
        ReplyID int IDENTITY(1,1) NOT NULL,
        FeedbackID int NOT NULL,
        SchoolID int NOT NULL,
        FromRegistrationID int NOT NULL,
        FromName nvarchar(200) NOT NULL,
        FromDesignation nvarchar(200) NOT NULL CONSTRAINT DF_StudentPortalFeedbackReply_Desig DEFAULT (N''),
        FromCategory nvarchar(20) NOT NULL,
        Body nvarchar(max) NOT NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedbackReply_At DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback_Reply PRIMARY KEY (ReplyID)
    );
    CREATE INDEX IX_Student_Portal_Feedback_Reply_Fb ON dbo.Student_Portal_Feedback_Reply (FeedbackID, CreatedAt);
END
IF OBJECT_ID(N'dbo.Student_Portal_Feedback_Reply_Read', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Student_Portal_Feedback_Reply_Read (
        ReplyID int NOT NULL,
        RegistrationID int NOT NULL,
        ReadAt datetime NOT NULL CONSTRAINT DF_StudentPortalFeedbackReplyRead_At DEFAULT (GETDATE()),
        CONSTRAINT PK_Student_Portal_Feedback_Reply_Read PRIMARY KEY (ReplyID, RegistrationID)
    );
END
""", con) { CommandTimeout = 20 };
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static bool CanMoveStatus(string current, string next, bool isMainAdmin)
    {
        if (string.Equals(current, next, StringComparison.OrdinalIgnoreCase))
            return true;
        if (next == "Processing")
            return current is "Pending";
        if (next == "Solved")
            return current is "Processing" || (isMainAdmin && current is "Pending");
        return false;
    }

    private static StudentFeedbackResult Fail(string error) => new() { Succeeded = false, Error = error };

    private static string? NormalizeKind(string? kind)
    {
        var text = (kind ?? "").Trim();
        if (text.Equals("Complaint", StringComparison.OrdinalIgnoreCase)
            || text.Equals("অভিযোগ", StringComparison.OrdinalIgnoreCase))
            return "Complaint";
        if (text.Equals("Suggestion", StringComparison.OrdinalIgnoreCase)
            || text.Equals("পরামর্শ", StringComparison.OrdinalIgnoreCase))
            return "Suggestion";
        return null;
    }

    private static string? NormalizeStatus(string? status)
    {
        var text = (status ?? "").Trim();
        if (text.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            || text.Equals("পেন্ডিং", StringComparison.OrdinalIgnoreCase))
            return "Pending";
        if (text.Equals("Processing", StringComparison.OrdinalIgnoreCase)
            || text.Equals("প্রসেসিং", StringComparison.OrdinalIgnoreCase))
            return "Processing";
        if (text.Equals("Solved", StringComparison.OrdinalIgnoreCase)
            || text.Equals("সলভ", StringComparison.OrdinalIgnoreCase)
            || text.Equals("সলভ হয়েছে", StringComparison.OrdinalIgnoreCase)
            || text.Equals("সমাধান", StringComparison.OrdinalIgnoreCase))
            return "Solved";
        return string.IsNullOrWhiteSpace(text) ? "Pending" : null;
    }

    private static bool IsStudent(SessionSnapshot session) =>
        session.IsStudent && session.StudentID > 0 && session.SchoolID > 0;

    private static bool IsOfficeRole(SessionSnapshot session) =>
        string.Equals(session.Role, "Admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(session.Role, "Sub-Admin", StringComparison.OrdinalIgnoreCase);

    private static int ToInt(object? value)
    {
        if (value is null or DBNull) return 0;
        return Convert.ToInt32(value);
    }

    private static int ReadColInt(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (!string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                continue;
            return ToInt(reader.GetValue(i));
        }
        return 0;
    }

    private static string ReadColText(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (!string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                continue;
            return Text(reader.GetValue(i));
        }
        return "";
    }

    private static DateTime? ReadColDate(SqlDataReader reader, string name)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (!string.Equals(reader.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                continue;
            var value = reader.GetValue(i);
            return value is DateTime d ? d : null;
        }
        return null;
    }

    private static string Text(object? value) =>
        value is null or DBNull ? "" : Convert.ToString(value)?.Trim() ?? "";

    private sealed class OfficeActor
    {
        public int RegistrationID { get; set; }
        public string Category { get; set; } = "";
        public bool IsMainAdmin =>
            string.Equals(Category, "Admin", StringComparison.OrdinalIgnoreCase);
        public bool CanAssign =>
            IsMainAdmin || string.Equals(Category, "Sub-Admin", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class SchoolAdminRow
    {
        public int RegistrationID { get; set; }
        public string UserName { get; set; } = "";
    }
}
