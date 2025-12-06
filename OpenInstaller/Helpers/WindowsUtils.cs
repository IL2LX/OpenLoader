using System;
using System.Diagnostics;
using System.Security.Principal;

namespace OpenInstaller.Helpers
{
    internal class WindowsUtils
    {
        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        public static void RestartAsAdmin()
        {
            try
            {
                string exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                {
                    Verb = "runas",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch
            {
            }
        }
    }
}
