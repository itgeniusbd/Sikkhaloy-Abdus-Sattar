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
                    File.AppendAllText(path, line + Environment.NewLine);
                }
            }
            catch
            {
                // ignored
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
