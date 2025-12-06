using System.IO;
using System.Reflection;

namespace OpenLoader.CPP2IL
{
    internal class Libs
    {
        internal static void Load()
        {
            Assembly.LoadFrom(Path.Combine(Generator.GetOLPath(), "0harmony.dll"));
        }
    }
}
