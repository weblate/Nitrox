using NitroxClient.MonoBehaviours.Gui.MainMenu;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxBootstrapper : MonoBehaviour
    {
        internal static NitroxBootstrapper Instance;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            gameObject.AddComponent<SceneCleanerPreserve>();
            gameObject.AddComponent<MainMenuMods>();

            Application.runInBackground = true;
            Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");

#if DEBUG
            EnableDeveloperFeatures();
            CreateDebugger();
#endif
        }

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer tools.");
            PlatformUtils.SetDevToolsEnabled(true);
        }

        private void CreateDebugger()
        {
            GameObject debugger = new GameObject();
            debugger.name = "Debug manager";
            debugger.AddComponent<NitroxDebugManager>();
            debugger.transform.SetParent(transform);
        }
    }
}
