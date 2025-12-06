using OpenLoader.Attributes;
using System;

namespace OpenLoader.Modules
{
    public class Mod
    {
        public Mod()
        {
        }

        internal Mod(Type type) => this.type = type;

        public Type type { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Author { get; private set; }

        public ModManager ModuleManager { get; private set; }

        public void Unload() => this.ModuleManager.UnloadModule(this);

        internal void Initialize(ModInfo moduleInfo, ModManager moduleManager)
        {
            this.Name = moduleInfo.Name;
            this.Version = moduleInfo.Version;
            this.Author = moduleInfo.Author;
            this.ModuleManager = moduleManager;
        }

        public void ValidateInitialization()
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.Version) || string.IsNullOrEmpty(this.Author))
                throw new InvalidOperationException("Module not properly initialized.");
        }

        public virtual void OnApplicationStart()
        {
            try
            {
                this.ValidateInitialization();
                Console.WriteLine($"{this.Name} ({this.Version}) by {this.Author} : OnApplicationStart exécuté !");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in OnApplicationStart: " + ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{this.Name} ({this.Version}) by {this.Author}";
        }
    }
}

