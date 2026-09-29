using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.LocalData.Sync;

public sealed partial class SyncEngine
{
    private readonly HashSet<int> _photoFetchInFlight = [];
    private readonly object _photoGate = new();

    public string? GetStudentPhotoDataUrl(int schoolId, int studentId) =>
        studentId <= 0 ? null : ReadImageDataUrl(StudentPhotoPath(schoolId, studentId));

    public string? GetGuardianPhotoDataUrl(int schoolId, int studentId) =>
        studentId <= 0 ? null : ReadImageDataUrl(GuardianPhotoPath(schoolId, studentId));

    public void SaveStudentPhoto(int schoolId, int studentId, string? dataUrl) =>
        WritePhoto(StudentPhotoPath(schoolId, studentId), schoolId, studentId, dataUrl);

    public void SaveGuardianPhoto(int schoolId, int studentId, string? dataUrl) =>
        WritePhoto(GuardianPhotoPath(schoolId, studentId), schoolId, studentId, dataUrl);

    public string? GetExamSignDataUrl(int schoolId, string kind) =>
        schoolId <= 0 ? null : ReadImageDataUrl(ExamSignPath(schoolId, kind));

    public void SaveExamSign(int schoolId, string kind, string? dataUrl)
    {
        if (schoolId <= 0 || string.IsNullOrWhiteSpace(dataUrl)) return;
        WritePhoto(ExamSignPath(schoolId, kind), schoolId, 1, dataUrl);
    }

    private static void WritePhoto(string path, int schoolId, int studentId, string? dataUrl)
    {
        if (schoolId <= 0 || studentId <= 0 || string.IsNullOrWhiteSpace(dataUrl))
            return;
        try
        {
            var bytes = DecodeImageBytes(dataUrl);
            if (bytes.Length == 0)
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }
        catch
        {
        }
    }

    public void ApplyCachedStudentPhotos(int schoolId, IEnumerable<StudentDto> students)
    {
        foreach (var student in students)
        {
            if (!string.IsNullOrWhiteSpace(student.PhotoDataUrl))
                continue;
            if (student.ServerId is int id && id > 0)
                student.PhotoDataUrl = GetStudentPhotoDataUrl(schoolId, id);
        }
    }

    public void CacheStudentPhotos(int schoolId, IEnumerable<SmStudentRowDto> rows)
    {
        foreach (var row in rows)
        {
            if (row.StudentID <= 0)
                continue;
            if (!string.IsNullOrWhiteSpace(row.PhotoDataUrl))
                SaveStudentPhoto(schoolId, row.StudentID, row.PhotoDataUrl);
            if (!string.IsNullOrWhiteSpace(row.GuardianPhotoDataUrl))
                SaveGuardianPhoto(schoolId, row.StudentID, row.GuardianPhotoDataUrl);
        }
    }

    public async Task EnsureStudentPhotosAsync(
        int schoolId,
        IEnumerable<StudentDto> students,
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        var list = students as IList<StudentDto> ?? students.ToList();
        if (schoolId <= 0 || list.Count == 0)
            return;

        ApplyCachedStudentPhotos(schoolId, list);
        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        List<int> missing;
        lock (_photoGate)
        {
            missing = list
                .Where(x => string.IsNullOrWhiteSpace(x.PhotoDataUrl) && x.ServerId is > 0)
                .Select(x => x.ServerId!.Value)
                .Distinct()
                .Where(id => _photoFetchInFlight.Add(id))
                .ToList();
        }
        if (missing.Count == 0)
            return;

        const int batchSize = 40;
        var byId = new Dictionary<int, string>();
        try
        {
            for (var i = 0; i < missing.Count; i += batchSize)
            {
                var chunk = missing.Skip(i).Take(batchSize).ToList();
                try
                {
                    var photos = await _api.GetStudentPhotosAsync(accessToken, chunk, cancellationToken);
                    foreach (var photo in photos)
                    {
                        if (photo.StudentID <= 0 || string.IsNullOrWhiteSpace(photo.PhotoDataUrl))
                            continue;
                        byId[photo.StudentID] = photo.PhotoDataUrl;
                        SaveStudentPhoto(schoolId, photo.StudentID, photo.PhotoDataUrl);
                    }
                }
                catch
                {
                    break;
                }
            }
        }
        finally
        {
            lock (_photoGate)
            {
                foreach (var id in missing)
                    _photoFetchInFlight.Remove(id);
            }
        }

        if (byId.Count == 0)
            return;

        foreach (var student in list)
        {
            if (!string.IsNullOrWhiteSpace(student.PhotoDataUrl))
                continue;
            if (student.ServerId is int id && byId.TryGetValue(id, out var url))
                student.PhotoDataUrl = url;
        }
    }

    private static string StudentPhotoPath(int schoolId, int studentId) =>
        Path.Combine(HybridFolder(), "photos", schoolId.ToString(), studentId + ".jpg");

    private static string GuardianPhotoPath(int schoolId, int studentId) =>
        Path.Combine(HybridFolder(), "photos", schoolId.ToString(), studentId + "-g.jpg");

    private static string ExamSignPath(int schoolId, string kind)
    {
        var safe = (kind ?? "").Trim().ToLowerInvariant();
        if (safe is not ("teacher" or "guardian" or "principal"))
            safe = "sign";
        return Path.Combine(HybridFolder(), "photos", schoolId.ToString(), "sign-" + safe + ".jpg");
    }
}
