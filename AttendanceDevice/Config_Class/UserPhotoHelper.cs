using System;
using System.IO;

namespace AttendanceDevice.Config_Class
{
    internal static class UserPhotoHelper
    {
        private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".JPG", ".JPEG", ".PNG" };

        /// <summary>Embedded default avatar when no local photo file exists.</summary>
        public const string DefaultPhotoUri = "pack://application:,,,/Resources/Default.png";

        public static bool PhotoExists(string imageFolder, string userId)
        {
            if (string.IsNullOrWhiteSpace(imageFolder) || string.IsNullOrWhiteSpace(userId))
                return false;

            var folder = imageFolder.Trim();
            if (!Directory.Exists(folder))
                return false;

            foreach (var ext in Extensions)
            {
                var path = Path.Combine(folder, userId + ext);
                if (File.Exists(path) && new FileInfo(path).Length > 0)
                    return true;
            }

            return false;
        }

        public static string GetPhotoPath(string imageFolder, string userId)
        {
            if (string.IsNullOrWhiteSpace(imageFolder) || string.IsNullOrWhiteSpace(userId))
                return null;

            return Path.Combine(imageFolder.Trim(), userId + ".jpg");
        }

        public static string ResolvePhotoUri(string imageFolder, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return DefaultPhotoUri;

            if (string.IsNullOrWhiteSpace(imageFolder))
                return DefaultPhotoUri;

            var folder = imageFolder.Trim();
            if (!Directory.Exists(folder))
                return DefaultPhotoUri;

            foreach (var ext in Extensions)
            {
                var path = Path.Combine(folder, userId + ext);
                if (!File.Exists(path))
                    continue;

                return new Uri(path, UriKind.Absolute).AbsoluteUri;
            }

            return DefaultPhotoUri;
        }
    }
}
