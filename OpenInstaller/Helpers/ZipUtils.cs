using System.IO;
using System.IO.Compression;

namespace OpenInstaller.Helpers
{
    internal class ZipUtils
    {
        public static bool ExtractZip(string zipFilePath, string extractToDirectory)
        {
            if (string.IsNullOrEmpty(zipFilePath) || !File.Exists(zipFilePath))
                return false;
            try
            {
                Directory.CreateDirectory(extractToDirectory);
                ZipFile.ExtractToDirectory(zipFilePath, extractToDirectory);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidZip(string zipFilePath)
        {
            if (string.IsNullOrEmpty(zipFilePath) || !File.Exists(zipFilePath))
                return false;
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    return archive.Entries.Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
