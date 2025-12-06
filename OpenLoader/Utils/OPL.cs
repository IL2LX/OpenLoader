using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenLoader.Utils
{
    public static class OPL
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

        private const int STD_OUTPUT_HANDLE = -11;
        private const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        public static void Alloc()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            if (consoleWindow == IntPtr.Zero)
            {
                AllocConsole();
                TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(writer);
                Console.SetError(writer);
                Console.OutputEncoding = Encoding.UTF8;
                Console.CursorVisible = false;
                Console.Title = $"OpenLoader";
                EnableVirtualTerminalProcessing();
                Console.Clear();
            }
        }
        static void EnableVirtualTerminalProcessing()
        {
            IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(handle, out int mode))
            {
                Console.WriteLine("Failed to get console mode.");
                return;
            }
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            if (!SetConsoleMode(handle, mode))
            {
                Console.WriteLine("Failed to enable VT processing.");
            }
        }
        public static void Log(string Name, string Content, bool shouldgradient = false)
        {
            DateTime now = DateTime.Now;
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(now.ToString("HH:mm"));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] [");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            if (shouldgradient)
            {
                Console.Write($"] ");
                LogGradient(Content);
            }
            else
            {
                Console.Write($"] {Content}\n");
            }
            Console.ResetColor();
        }

        public static void E(Exception ex)
        {
            DateTime now = DateTime.Now;

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("\n========== ERROR ==========");
            Console.WriteLine($"TIME: {now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"MESSAGE: {ex.Message}");

            if (ex.InnerException != null)
                Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");

            Console.WriteLine("STACK TRACE:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("FULL EXCEPTION:");
            Console.WriteLine(ex);
            Console.WriteLine("=========== END ===========\n");

            Console.ResetColor();
            Console.ReadKey();
        }


        public static void ModLog(string Name, string Content, bool shouldGradient = false)
        {
            DateTime now = DateTime.Now;
            string callingAssembly = "Unknown";
            try
            {
                StackTrace stackTrace = new StackTrace();
                MethodBase method = stackTrace.GetFrame(1).GetMethod();
                callingAssembly = method.DeclaringType?.Assembly.GetName().Name ?? "Unknown";
            }
            catch { }
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(now.ToString("HH:mm"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{callingAssembly}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{Name}] ");
            if (shouldGradient)
            {
                LogGradient(Content);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Content);
            }
            Console.ResetColor();
        }
        public static void DrawProgressBar(int progress, int total, int barSize = 20)
        {
            float ratio = (float)progress / total;
            int filled = (int)(ratio * barSize);
            int empty = barSize - filled;
            Console.Write("\r[");
            for (int i = 0; i < filled; i++)
            {
                float t = (float)i / filled;
                int r = (int)(255 * (1 - t));
                int g = (int)(255 * t);
                int b = 0;
                Console.Write($"\x1b[38;2;{r};{g};{b}m=");
            }
            for (int i = 0; i < empty; i++)
            {
                Console.Write("\x1b[38;2;100;100;100m-");
            }
            Console.Write($"\x1b[0m] {progress}/{total} ({ratio:P0})");
        }
        static void LogGradient(string text)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                int red = 255 - (i * 255 / length);
                int green = 0;
                int blue = i * 255 / length;

                Console.Write($"\x1b[38;2;{red};{green};{blue}m{text[i]}");
            }
            Console.WriteLine("\x1b[0m");
        }
    }
}
