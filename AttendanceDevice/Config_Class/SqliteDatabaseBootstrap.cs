using System;
using System.Data.SQLite;
using System.IO;

namespace AttendanceDevice.Config_Class
{
    internal static class SqliteDatabaseBootstrap
    {
        private const string TemplateFileName = "SikkhaloyAppDB.template.db";

        public static void EnsureDatabase()
        {
            Directory.CreateDirectory(AppPaths.LocalDataDirectory);

            string dbPath = AppPaths.DatabasePath;

            string templatePath = Path.Combine(AppPaths.AppDirectory, "Database", TemplateFileName);

            if (!File.Exists(templatePath))
                templatePath = Path.Combine(AppPaths.AppDirectory, TemplateFileName);

            if (!File.Exists(templatePath))
                return;

            if (!NeedsTemplateSeed(dbPath))
                return;

            File.Copy(templatePath, dbPath, true);
        }

        private static bool NeedsTemplateSeed(string dbPath)
        {
            if (!File.Exists(dbPath))
                return true;

            if (new FileInfo(dbPath).Length == 0)
                return true;

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'User_Info'";
                        return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
                    }
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
