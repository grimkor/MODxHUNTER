using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using modxhunter.modules;
using modxhunter.modules.ColliderViewer;
using modxhunter.modules.OverlayMenu;
using modxhunter.modules.PostProcessing;
using modxhunter.modules.TextureLoader;

namespace modxhunter;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        var harmony = new Harmony("Base.MODxHUNTER.Mod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        LoadConfig();
        InjectBehaviours();
        MenuManager.Instance.Init();
        PostProcessing.Instance.Init();
        KeyStrokeListener.Init();
        TextureManager.Instance.Init();
    }

    private void InjectBehaviours()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ColliderViewer.ColliderViewerBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<GreenScreen.GreenScreenBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<KeyStrokeListener>();
        ClassInjector.RegisterTypeInIl2Cpp<WindowBase>();
        ClassInjector.RegisterTypeInIl2Cpp<MenuOverlay>();
        ClassInjector.RegisterTypeInIl2Cpp<MenuPostProcessing>();
        ClassInjector.RegisterTypeInIl2Cpp<MenuGreenScreen>();
        ClassInjector.RegisterTypeInIl2Cpp<MenuManager.MenuManagerBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<PostProcessing.PostProcessingBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<MenuTextureLoader>();
        ClassInjector.RegisterTypeInIl2Cpp<TextureManagerBehaviour>();
    }
    internal static ConfigEntry<bool> DiffusionEnable;
    internal static ConfigEntry<float> DiffusionValue;
    internal static ConfigEntry<bool> BloomEnable;
    internal static ConfigEntry<bool> SubpixelAntialiasing;
    internal static ConfigEntry<bool> TextureLoadingEnabled;

    public void LoadConfig()
    {
        DiffusionEnable = Config.Bind("Postprocessing", "DiffusionEnable", true, "Enable Diffusion");
        DiffusionValue = Config.Bind("Postprocessing", "DiffusionValue", 0.45f, "Diffusion Amount (default 0.45)");
        BloomEnable = Config.Bind("Postprocessing", "BloomEnable", true, "Enable Bloom");
        SubpixelAntialiasing = Config.Bind("Postprocessing", "SubpixelAntialiasing", false, "Enable subpixel Antialiasing");
        TextureLoadingEnabled = Config.Bind("TextureLoading", "TextureLoadingEnabled", false, "Enable Loading Custom Textures");
    }
}
