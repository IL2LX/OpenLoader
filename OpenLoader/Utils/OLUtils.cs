using System.IO;

namespace OpenLoader.Utils
{
    public class OLUtils
    {
        public static void CheckDirectories()
        {
            foreach (var dir in new[] { "Generator", "Misc", "Mods", "UserData" })
            {
                string path = Path.Combine("OL", dir);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        public static string GetUserDataDir()
        {
            if (!Directory.Exists(Path.Combine("OL", "UserData")))
            {
                Directory.CreateDirectory(Path.Combine("OL", "UserData"));
            }
            return Path.GetFullPath(Path.Combine("OL", "UserData"));
        }
    }
}
