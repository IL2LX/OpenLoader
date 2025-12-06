using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenLoader.Utils
{
    internal static class CrashHandler
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
        private static readonly object _lock = new object();

        public static event Action<Exception> OnCrash;

        public static void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, "Unhandled domain exception");

                // Attempt to prevent crash
                if (!e.IsTerminating)
                {
                    Console.WriteLine("Attempting to continue after exception...");
                }
            }
            else
            {
                LogMessage($"Unhandled non-Exception object: {e.ExceptionObject}");
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception, "Unobserved task exception");
            e.SetObserved(); // prevents the app from crashing
        }

        private static void HandleException(Exception ex, string context = null)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LogFile));
                    var sb = new StringBuilder();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine($"[{DateTime.Now}] CRASH: {context ?? "Exception"}");
                    sb.AppendLine($"Type: {ex.GetType()} | Message: {ex.Message}");
                    sb.AppendLine("Stack Trace:");
                    sb.AppendLine(ex.StackTrace);

                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        sb.AppendLine($"\nInner Exception: {inner.GetType()} - {inner.Message}");
                        sb.AppendLine(inner.StackTrace);
                        inner = inner.InnerException;
                    }

                    sb.AppendLine("\nEnvironment Info:");
                    sb.AppendLine($"OS Version: {Environment.OSVersion}");
                    sb.AppendLine($".NET Version: {Environment.Version}");
                    sb.AppendLine($"Machine: {Environment.MachineName}");
                    sb.AppendLine($"User: {Environment.UserName}");

                    sb.AppendLine("\nLoaded Assemblies:");
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        sb.AppendLine($"{asm.GetName().Name} ({asm.GetName().Version})");

                    sb.AppendLine(new string('=', 80) + "\n");
                    File.AppendAllText(LogFile, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
            }
            finally
            {
                OnCrash?.Invoke(ex);
            }
        }

        private static void LogMessage(string msg)
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LogFile));
                    File.AppendAllText(LogFile, $"[{DateTime.Now}] {msg}\n", Encoding.UTF8);
                }
            }
            catch { }
        }

        public static void SafeExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                HandleException(ex, "SafeExecute caught exception");
            }
        }

        public static T SafeExecute<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                HandleException(ex, "SafeExecute<T> caught exception");
                return default;
            }
        }
    }
}
