using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using modxhunter.modules;
using modxhunter.modules.ColliderViewer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace modxhunter;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        var harmony = new Harmony("Base.MODxHUNTER.Mod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        ClassInjector.RegisterTypeInIl2Cpp<ColliderViewer.ColliderViewerBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<KeyStrokeListener>();
        var go = new GameObject() { name = "mxh-KeyStrokeListener" };
        go.hideFlags = HideFlags.HideAndDontSave;
        Object.DontDestroyOnLoad(go);
        var ksl = go.AddComponent<KeyStrokeListener>();
        ksl.enabled = true;
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

}

