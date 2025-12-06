using OpenLoader.CPP2IL;
using OpenLoader.Hooks;
using OpenLoader.Modules;
using OpenLoader.Utils;
using SDK.Utils;

namespace OpenLoader
{
    public class Entry
    {
        internal static Entry _self { get; set; }
        public static ModManager _Mod { get; set; }
        public static bool _modulesLoaded = false;
        public static int Run(string arg)
        {
            OPL.Alloc(); 
            _self = new Entry();
            _self.Awake();
            return 0;
        }
        public void Awake()
        {
            if (!_modulesLoaded)
            {
                CrashHandler.Initialize();
                OLUtils.CheckDirectories();
                Libs.Load();
                Generator.Start();
                IL2CPP.Start();
                CoroutineManager.Start();
                OnSceneLoadedHook.Start();
                _Mod = new ModManager();
                int OL_count = _Mod.FindModules().Count;
                OPL.Log("ModManager", string.Format("{0} Mod{1} loaded.", OL_count, (OL_count == 1) ? "" : "s"));
                CrashHandler.SafeExecute(() => ModManager.Start());
                ModManager.OnUpdateTH();
                _modulesLoaded = true;
            }
        }
    }
}
