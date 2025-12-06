using System;
using System.Diagnostics;
using System.IO;

namespace OpenInstaller.Helpers
{
    internal class ProcessUtils
    {
        public static void HandleVRCLaunchEAC()
        {
            ConsoleUtils.Log("Launcher","Starting VRChat...");
            ConsoleUtils.Log("Launcher", "Do you want to start in VR mode? (y/n): ");
            string userInput = Console.ReadLine()?.Trim().ToLower();
            string arguments = userInput == "y" ? "" : "--no-vr";
            StartPro(FilesUtils.GetVRChatPath(), "start_protected_game.exe", arguments);
            Process.GetCurrentProcess().Kill();
        }

        public static void StartPro(string path, string exename, string arguments)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(path, exename),
                Arguments = arguments,
                UseShellExecute = true
            });
        }
    }
}
