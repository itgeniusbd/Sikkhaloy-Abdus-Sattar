using System;
using System.Diagnostics;
using System.IO;

namespace ZKdllRegistrationApp
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string targetDir = Path.Combine(windowsDir, Environment.Is64BitOperatingSystem ? "SysWOW64" : "System32");
                string sourceDir = args.Length > 0 && Directory.Exists(args[0])
                    ? args[0]
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll");

                if (!Directory.Exists(sourceDir))
                {
                    Console.WriteLine("SDK source folder not found: " + sourceDir);
                    return 1;
                }

                foreach (string dll in Directory.GetFiles(sourceDir, "*.dll"))
                {
                    string targetPath = Path.Combine(targetDir, Path.GetFileName(dll));
                    File.Copy(dll, targetPath, true);
                }

                string zkemkeeperPath = Path.Combine(targetDir, "zkemkeeper.dll");
                if (!File.Exists(zkemkeeperPath))
                {
                    Console.WriteLine("zkemkeeper.dll was not copied.");
                    return 1;
                }

                var reg = new Process
                {
                    StartInfo =
                    {
                        FileName = "regsvr32.exe",
                        Arguments = "/s \"" + zkemkeeperPath + "\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                reg.Start();
                reg.WaitForExit();
                reg.Close();

                return reg.ExitCode == 0 ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
