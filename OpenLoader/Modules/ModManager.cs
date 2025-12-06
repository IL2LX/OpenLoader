using OpenLoader.Attributes;
using OpenLoader.CPP2IL;
using OpenLoader.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OpenLoader.Modules
{
    public class ModManager
    {
        private List<Mod> _modules { get; } = new List<Mod>();

        public ReadOnlyCollection<Mod> Modules => _modules.AsReadOnly();

        public ReadOnlyCollection<Mod> FindModules()
        {
            string modsPath = Path.Combine(Generator.GetOLPath(), "Mods");
            if (!Directory.Exists(modsPath))
            {
                try
                { 
                    Directory.CreateDirectory(modsPath); 
                    OPL.Log("Helper", "OL/Mods folder did not exist, created folder."); 
                }
                catch (Exception ex) 
                { 
                    OPL.Log("ModManager", $"Failed to create Mods folder: {ex}"); 
                    return Modules; 
                }
            }
            string[] files;
            try 
            {
                files = Directory.GetFiles(modsPath, "*.dll"); 
            }
            catch (Exception ex) 
            { 
                OPL.Log("ModManager", $"Error accessing Mods folder: {ex}"); return Modules; 
            }
            foreach (string filePath in files)
            {
                Assembly assembly = null;
                string fileName = Path.GetFileName(filePath);
                try { assembly = Assembly.UnsafeLoadFrom(filePath); }
                catch (Exception ex1)
                {
                    OPL.Log("ModManager", $"Failed to load assembly {fileName} via UnsafeLoadFrom: {ex1}");
                    try 
                    { 
                        assembly = Assembly.Load(File.ReadAllBytes(filePath)); 
                    }
                    catch (Exception ex2) 
                    { 
                        OPL.Log("ModManager", $"Failed to load assembly {fileName} via Load(byte[]): {ex2}"); 
                        continue; 
                    }
                }
                if (assembly == null) 
                    continue;
                Type[] types;
                try 
                { 
                    types = assembly.GetTypes(); 
                }
                catch (ReflectionTypeLoadException ex) 
                { 
                    types = ex.Types.Where(t => t != null).ToArray(); 
                    OPL.Log("ModManager", $"ReflectionTypeLoadException while getting types from {fileName}: {ex}"); 
                }
                catch (Exception ex) 
                { 
                    OPL.Log("ModManager", $"Error getting types from {fileName}: {ex}"); 
                    continue; 
                }
                foreach (Type type in types.Where(t => t.IsSubclassOf(typeof(Mod))))
                {
                    try
                    {
                        if (type.GetCustomAttributes(typeof(ModInfo), true).FirstOrDefault() is ModInfo info)
                        {
                            Mod mod = new Mod(type);
                            _modules.Add(mod);
                            try { mod.Initialize(info, this); }
                            catch (Exception ex) 
                            { 
                                OPL.Log("ModManager", $"Failed to initialize mod {mod}: {ex}"); 
                            }
                        }
                    }
                    catch (Exception ex) 
                    { 
                        OPL.Log("ModManager", $"Error processing type {type.FullName} in {fileName}: {ex}"); 
                    }
                }
            }
            return Modules;
        }


        internal static void Start()
        {
            LoadMethode("OnApplicationStart");
        }

        internal static void OnSceneLoaded()
        {
            LoadMethode("OnSceneLoaded");
        }

        public static void OnUpdateTH()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        LoadMethode("OnUpdate");
                    }
                    catch (Exception ex)
                    {
                        OPL.Log("ModManager", $"Error in OnUpdate thread: {ex}");
                    }
                    try
                    {
                        LoadMethode("OnLateUpdate");
                    }
                    catch (Exception ex)
                    {
                        OPL.Log("ModManager", $"Error in OnLateUpdate thread: {ex}");
                    }
                    Thread.Sleep(16);
                }
            })
            {
                IsBackground = true,
                Name = "ModManager_OnUpdateThread"
            };
            thread.Start();
        }

        internal static void LoadMethode(string methodName)
        {
            foreach (var module in Entry._Mod.Modules)
            {
                InvokeMethod(module, methodName);
            }
        }

        internal static void InvokeMethod(dynamic module, string methodName)
        {
            try
            {
                var method = module.type.GetMethod(methodName);
                method?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                OPL.Log("ModManager", $"Error Invoking {methodName} in mod {module}.");
                OPL.E(ex);
            }
        }

        public void UnloadModule(Mod module)
        {
            try
            {
                if (_modules.Contains(module))
                {
                    _modules.Remove(module);
                    OPL.Log("ModManager", $"{module} unloaded.");
                }
            }
            catch (Exception ex)
            {
                OPL.Log("ModManager", $"Failed to unload module {module}: {ex}");
            }
        }
    }
}
