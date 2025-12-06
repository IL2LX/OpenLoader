using System;
using System.Diagnostics;
using System.IO;

namespace OpenInstaller.Helpers
{
    internal class InjHelper
    {
        internal static void SetupIt()
        {
            try
            {
                File.Copy(FilesUtils.GetVRChatPath() + "\\OL\\inj.dll", @"C:\Program Files (x86)\Steam\GameOverlayRenderer64.dll", overwrite: true);
            }
            catch (Exception ex)
            {
                ConsoleUtils.Log("Inj", "It failed:\n" + ex);
                ConsoleUtils.Log("Inj", "Try restarting steam and retry.");
                Console.ReadKey();
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
