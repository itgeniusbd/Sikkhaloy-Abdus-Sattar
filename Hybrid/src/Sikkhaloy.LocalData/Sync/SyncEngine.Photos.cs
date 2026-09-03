using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.LocalData.Sync;

public sealed partial class SyncEngine
{
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
            if (student.ServerId is int id && id > 0)
                student.PhotoDataUrl = GetStudentPhotoDataUrl(schoolId, id);
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
