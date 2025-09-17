using System;
using System.Diagnostics;

namespace ZKdllRegistrationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Windows bit type check 64 ro 32
                var systemType = IntPtr.Size == 8 ? "SysWow64" : "System32";

                var filePath = $@"C:\Windows\{systemType}\zkemkeeper.dll";

                var argFileInfo = "";

                ////Show message boxes or Don't Show any message boxes. ('/s' : Specifies regsvr32 to run silently)  
                var showMessageBox = true;

                if (!showMessageBox) argFileInfo += "/s";

                argFileInfo += "\"" + filePath + "\"";

                //This file registers .dll files as command components in the registry.
                var reg = new Process
                {
                    StartInfo =
                    {
                        FileName = "regsvr32.exe",
                        Arguments = argFileInfo,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                reg.Start();
                reg.WaitForExit();
                reg.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
