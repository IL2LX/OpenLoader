using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace OpenInstaller.Helpers
{
    internal class FilesUtils
    {
        public static string GetVRChatPath()
        {
            try
            {
                string steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                if (!string.IsNullOrEmpty(steamPath))
                {
                    string vrchatPath = Path.Combine(steamPath, @"steamapps\common\VRChat");
                    if (Directory.Exists(vrchatPath))
                        return vrchatPath;
                }
            }
            catch { }
            string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\VRChat";
            if (Directory.Exists(defaultPath))
                return defaultPath;
            return null;
        }

        public static bool IsValidInstallation()
        {
            string vrchatPath = GetVRChatPath();
            if (string.IsNullOrEmpty(vrchatPath) || !Directory.Exists(vrchatPath))
                return false;
            string olFolder = Path.Combine(vrchatPath, "OL");
            if (!Directory.Exists(olFolder))
                return false;
            string[] requiredSubfolders = { "Mods", "Misc", "Generator" };
            foreach (string subfolder in requiredSubfolders)
            {
                if (!Directory.Exists(Path.Combine(olFolder, subfolder)))
                    return false;
            }
            string openLoaderDll = Path.Combine(olFolder, "OpenLoader.dll");
            return File.Exists(openLoaderDll);
        }
        public static void DownloadShit()
        {
            string olPath = Path.Combine(GetVRChatPath(), "OL");
            string dllPath = Path.Combine(olPath, "OpenLoader.dll");
            string dllUrl = "https://example.com/OpenLoader.dll";
            string zipUrl = "https://example.com/OL.zip";
            string tempZip = Path.Combine(Path.GetTempPath(), "OL.zip");
            try
            {
                if (Directory.Exists(olPath))
                {
                    if (File.Exists(dllPath))
                    {
                        File.Delete(dllPath);
                    }
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(dllUrl, dllPath);
                        ConsoleUtils.Log("Updater", "Updated OpenLoader.");
                    }
                }
                else
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(zipUrl, tempZip);
                        ConsoleUtils.Log("Updater", "Installing OpenLoader...");
                    }
                    ZipFile.ExtractToDirectory(tempZip, olPath);
                    ConsoleUtils.Log("Updater", "OpenLoader installed.");
                    File.Delete(tempZip);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.ReadKey();
            }
        }
    }
}
