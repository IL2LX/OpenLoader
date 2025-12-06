using OpenLoader.Modules;
using OpenLoader.Utils;
using System;
using System.Threading;
using UnityEngine.SceneManagement;

namespace OpenLoader.Hooks
{
    internal class OnSceneLoadedHook
    {
        private static string lastSceneName = "";
        public static void Start()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        string currentScene = SceneManager.GetActiveScene().name;
                        if (currentScene != lastSceneName)
                        {
                            lastSceneName = currentScene;
                            ModManager.OnSceneLoaded();
                        }
                    }
                    catch (Exception ex)
                    {
                        OPL.Log("OnSceneLoaded", $"Error in scene check thread: {ex}");
                    }
                    Thread.Sleep(300);
                }
            })
            {
                IsBackground = true,
                Name = "SceneCheckThread"
            };
            thread.Start();
        }
    }
}
