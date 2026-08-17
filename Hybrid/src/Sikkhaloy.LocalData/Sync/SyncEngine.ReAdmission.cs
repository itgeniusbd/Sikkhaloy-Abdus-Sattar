using Microsoft.EntityFrameworkCore;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

public sealed partial class SyncEngine
{
    public async Task<IReadOnlyList<ReAdmissionCandidateDto>> ListReAdmissionCandidatesAsync(
        string accessToken,
        int yearId,
        int classId,
        int sectionId,
        int groupId,
        int shiftId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || yearId <= 0 || classId <= 0)
            return [];
        if (!await _api.PingAsync(cancellationToken))
            throw new InvalidOperationException("readm.offline");
        return await _api.GetReAdmissionCandidatesAsync(
            accessToken, yearId, classId, sectionId, groupId, shiftId, cancellationToken);
    }

    public async Task<ReAdmissionAssignDto> GetReAdmissionAssignAsync(
        string accessToken, int studentId, int fromYearId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("readm.offline");
        if (!await _api.PingAsync(cancellationToken))
            throw new InvalidOperationException("readm.offline");
        return await _api.GetReAdmissionAssignAsync(accessToken, studentId, fromYearId, cancellationToken);
    }

    public async Task<IReadOnlyList<ReAdmissionSubjectDto>> ListReAdmissionSubjectsAsync(
        string accessToken, int classId, int groupId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || classId <= 0)
            return [];
        if (!await _api.PingAsync(cancellationToken))
            throw new InvalidOperationException("readm.offline");
        return await _api.GetReAdmissionSubjectsAsync(accessToken, classId, groupId, cancellationToken);
    }

    public async Task<ReAdmissionResult> FinishReAdmissionAsync(
        SessionSnapshot session, string accessToken, ReAdmissionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return new ReAdmissionResult { Succeeded = false, Error = "readm.offline" };
        if (!await _api.PingAsync(cancellationToken))
            return new ReAdmissionResult { Succeeded = false, Error = "readm.offline" };

        var result = await _api.FinishReAdmissionAsync(accessToken, request, cancellationToken);
        if (result.Succeeded && result.Student is not null)
            await ImportSyncedStudentAsync(result.Student, session, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<ReAdmissionExamDto>> ListReAdmissionExamsAsync(
        string accessToken, int yearId, int classId, bool cumulative, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || yearId <= 0 || classId <= 0)
            return [];
        if (!await _api.PingAsync(cancellationToken))
            throw new InvalidOperationException("readm.offline");
        return await _api.GetReAdmissionExamsAsync(accessToken, yearId, classId, cumulative, cancellationToken);
    }

    public async Task<IReadOnlyList<ReAdmissionPositionDto>> ListReAdmissionPositionsAsync(
        string accessToken, int yearId, int classId, int examId, bool cumulative, bool sectionWise, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || examId <= 0)
            return [];
        if (!await _api.PingAsync(cancellationToken))
            throw new InvalidOperationException("readm.offline");
        return await _api.GetReAdmissionPositionsAsync(
            accessToken, yearId, classId, examId, cumulative, sectionWise, cancellationToken);
    }

    public async Task<BulkReAdmissionResult> FinishBulkReAdmissionAsync(
        SessionSnapshot session, string accessToken, BulkReAdmissionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return new BulkReAdmissionResult { Succeeded = false, Error = "readm.offline" };
        if (!await _api.PingAsync(cancellationToken))
            return new BulkReAdmissionResult { Succeeded = false, Error = "readm.offline" };

        var result = await _api.FinishBulkReAdmissionAsync(accessToken, request, cancellationToken);
        if (result.Succeeded)
        {
            foreach (var student in result.Students)
                await ImportSyncedStudentAsync(student, session, cancellationToken);
        }

        return result;
    }

    public async Task ImportSyncedStudentAsync(
        StudentDto dto, SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        if (dto.EducationYearID != session.EducationYearID)
            return;

        dto.SchoolID = session.SchoolID;
        dto.UpdatedUtc = DateTime.UtcNow;
        dto.SyncStatus = SyncStatus.Synced;
        if (dto.LocalId == Guid.Empty)
            dto.LocalId = Guid.NewGuid();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var local = await db.Students.FirstOrDefaultAsync(
            x => x.SchoolID == dto.SchoolID
                 && x.ServerId == dto.ServerId
                 && x.EducationYearID == dto.EducationYearID,
            cancellationToken);
        if (local is null)
        {
            db.Students.Add(Map(dto, session.DeviceId));
        }
        else
        {
            dto.LocalId = local.LocalId;
            Copy(dto, local, session.DeviceId);
        }

        await db.SaveChangesAsync(cancellationToken);
        StateChanged?.Invoke();
    }
}
