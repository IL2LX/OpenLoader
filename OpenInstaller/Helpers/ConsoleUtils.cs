using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenInstaller.Helpers
{
    internal class ConsoleUtils
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
            TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Title = $"OpenInstaller";
            EnableVirtualTerminalProcessing();
            art();
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
            if (length == 0) return;
            float startR = 178, startG = 0, startB = 50;
            float endR = 255, endG = 178, endB = 190;
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / (length - 1);
                int red = (int)(startR + (endR - startR) * t);
                int green = (int)(startG + (endG - startG) * t);
                int blue = (int)(startB + (endB - startB) * t);
                Console.Write($"\x1b[38;2;{red};{green};{blue}m{text[i]}");
            }
            Console.WriteLine("\x1b[0m");
        }

        public static void art()
        {
            LogGradient(" ▄▄▄▄                       ▄▄▄▄▄                  ▄           ▀▀█    ▀▀█                 ");
            LogGradient("▄▀  ▀▄ ▄▄▄▄    ▄▄▄   ▄ ▄▄     █    ▄ ▄▄    ▄▄▄   ▄▄█▄▄   ▄▄▄     █      █     ▄▄▄    ▄ ▄▄ ");
            LogGradient("█    █ █▀ ▀█  █▀  █  █▀  █    █    █▀  █  █   ▀    █    ▀   █    █      █    █▀  █   █▀  ▀");
            LogGradient("█    █ █   █  █▀▀▀▀  █   █    █    █   █   ▀▀▀▄    █    ▄▀▀▀█    █      █    █▀▀▀▀   █    ");
            LogGradient(" █▄▄█  ██▄█▀  ▀█▄▄▀  █   █  ▄▄█▄▄  █   █  ▀▄▄▄▀    ▀▄▄  ▀▄▄▀█    ▀▄▄    ▀▄▄  ▀█▄▄▀   █    ");
            LogGradient("       █                                                                                  ");
            LogGradient("       ▀                                                           By FC SoftWare Team.   ");
            LogGradient("------------------------------------------------------------------------------------------");
        }
    }
}
