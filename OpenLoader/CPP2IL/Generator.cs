using Microsoft.SqlServer.Server;
using OpenLoader.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace OpenLoader.CPP2IL
{
    internal static class Generator
    {
        public static string outputFolder = Path.Combine(GetOLPath(), "Managed");
        internal static void Start()
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                GenerateIL();
            }
            else
            {
                LoadDlls();
            }
        }

        internal static void GenerateIL()
        {
            string vrcPath = GetVRCPath();
            OPL.Log("Generator", "Starting Cpp2IL Generation...");
            OPL.Log("Generator", $"Game path: {vrcPath}");
            OPL.Log("Generator", $"Output folder: {outputFolder}");
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(GetOLPath(), "Generator/Cpp2IL.exe"),
                Arguments = $"--game-path \"{vrcPath}\" --exe-name VRChat --output-as dummydll --output-to \"{outputFolder}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"[ERROR] {e.Data}");
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            LoadDlls();
        }

        internal static void LoadDlls()
        {
            string[] dllFiles = Directory.GetFiles(outputFolder, "*.dll", SearchOption.TopDirectoryOnly);
            Console.Write("Loading DLLs:");
            int total = dllFiles.Length;

            for (int i = 0; i < total; i++)
            {
                Assembly.LoadFrom(dllFiles[i]);
                OPL.DrawProgressBar(i + 1, total);
            }
            Console.WriteLine();
            OPL.Log("Generator", $"All {total} DLL(s) have been loaded.");
        }

        internal static string GetVRCPath()
            => @"C:\Program Files (x86)\Steam\steamapps\common\VRChat";

        internal static string GetOLPath()
            => Path.Combine(GetVRCPath(), "OL");
    }
}
