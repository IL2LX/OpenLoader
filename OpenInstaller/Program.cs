using OpenInstaller.Helpers;
using System;
using System.Diagnostics;

namespace OpenInstaller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleUtils.Alloc();
            if (!WindowsUtils.IsAdministrator())
            {
                Console.Beep(900, 320);
                Console.Beep(500, 120);
                ConsoleUtils.Log("Start", "Please click yes to continue.");
                WindowsUtils.RestartAsAdmin();
                Process.GetCurrentProcess().Kill();
            }
            if (FilesUtils.IsValidInstallation())
            {
                InjHelper.SetupIt();
                ProcessUtils.HandleVRCLaunchEAC();
            }
            else
            {
                Console.Beep(900,120);
                Console.Beep(500,120);
                ConsoleUtils.Log("Start", "Not a valid installation.");
                ConsoleUtils.Log("Start", "Are you sure you did put OL folder inte VRChat installation location below.");
                ConsoleUtils.Log("Start", FilesUtils.GetVRChatPath());
                Console.ReadKey();
            }
        }
    }
}
