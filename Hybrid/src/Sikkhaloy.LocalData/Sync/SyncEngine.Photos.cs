using Sikkhaloy.Shared.Students;

namespace Sikkhaloy.LocalData.Sync;

public sealed partial class SyncEngine
{
    public string? GetStudentPhotoDataUrl(int schoolId, int studentId) =>
        studentId <= 0 ? null : ReadImageDataUrl(StudentPhotoPath(schoolId, studentId));

    public void SaveStudentPhoto(int schoolId, int studentId, string? dataUrl)
    {
        if (schoolId <= 0 || studentId <= 0 || string.IsNullOrWhiteSpace(dataUrl))
            return;
        try
        {
            var bytes = DecodeImageBytes(dataUrl);
            if (bytes.Length == 0)
                return;
            var path = StudentPhotoPath(schoolId, studentId);
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
}
