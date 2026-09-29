using System;
using System.IO;

namespace AttendanceDevice.Config_Class
{
    internal static class StartupLogger
    {
        private static readonly object Sync = new object();

        public static void LogStage(string stage)
        {
            WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {stage}");
        }

        public static void LogFailure(string stage, Exception ex)
        {
            WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FAILED at {stage}{Environment.NewLine}{FormatException(ex)}");
        }

        private static void WriteLine(string line)
        {
            try
            {
                lock (Sync)
                {
                    var path = Path.Combine(AppPaths.LogsDirectory, "startup-error.log");
                    RotateIfLarge(path);
                    File.AppendAllText(path, line + Environment.NewLine);
                }
            }
            catch
            {
                // ignored
            }
        }

        private const long MaxLogBytes = 5L * 1024 * 1024;

        /// <summary>Keep the log bounded when the app runs for weeks: above 5 MB it becomes .old.</summary>
        private static void RotateIfLarge(string path)
        {
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists || info.Length < MaxLogBytes)
                    return;

                var old = path + ".old";
                if (File.Exists(old))
                    File.Delete(old);
                File.Move(path, old);
            }
            catch
            {
                // Logging must never break the app.
            }
        }

        private static string FormatException(Exception ex)
        {
            var parts = new System.Collections.Generic.List<string>();
            while (ex != null)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message))
                    parts.Add(ex.GetType().Name + ": " + ex.Message.Trim());
                ex = ex.InnerException;
            }

            return string.Join(Environment.NewLine + "  -> ", parts);
        }
    }
}
