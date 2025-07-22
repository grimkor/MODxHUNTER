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
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        LoadConfig();
        InjectBehaviours();
        MenuManager.Instance.Init();
        PostProcessing.Instance.Init();
        KeyStrokeListener.Init();
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
    }
        internal static ConfigEntry<bool> DiffusionEnable;
        internal static ConfigEntry<float> DiffusionValue;
        internal static ConfigEntry<bool> BloomEnable;
        internal static ConfigEntry<bool> SubpixelAntialiasing;

        public void LoadConfig()
        {
            DiffusionEnable = Config.Bind("Postprocessing", "DiffusionEnable", true, "Enable Diffusion");
            DiffusionValue = Config.Bind("Postprocessing", "DiffusionValue", 0.45f, "Diffusion Amount (default 0.45)");
            BloomEnable = Config.Bind("Postprocessing", "BloomEnable", true, "Enable Bloom");
            SubpixelAntialiasing = Config.Bind("Postprocessing", "SubpixelAntialiasing", false, "Enable subpixel Antialiasing");
        }
}


// [HarmonyPatch(typeof(CharaSelectManager), nameof(CharaSelectManager.GetAssetImgColorCostumes), new Type[] {typeof(string), typeof(int), typeof(int)})]
// public static class CharaSelectManager_GetAssetImgColorCostumes
// {
//     private static void Postfix(string charaId, int iColor, int iCostume, ref Sprite __result)
//     {
//         if (iCostume < 2 && iColor < 4) return;
//         var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
//         var spriteList = sprites.ToList();
//         var sprite = spriteList.First(s =>
//         {
//             return s.name == "T_ui_CharacterSelect_Img_p01a_d01_c01_D";
//         });
//         if (sprite)
//         {
//             Plugin.Log.LogInfo($"CharaSelectManager_GetAssetImgColorCostumes: SETTING {sprite.name}");
//             __result = sprite;
//         }
//     }
//     // private static void Postfix(string charaId, int iColor, int iCostume, ref Sprite __result)
//     // {
//     //     var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
//     //     var spriteList = sprites.ToList();
//     //     var sprite = spriteList.FirstOrDefault(s =>
//     //     {
//     //         // Plugin.Log.LogInfo($"CharaSelectManager_GetAssetImgColorCostumes: {s.name}");
//     //         return s.name == "T_ui_CharacterSelect_Img_p01a_d01_c01_D";
//     //     }, __result);
//     //     if (sprite)
//     //     {
//     //         __result = sprite;
//     //     }
//     // }
// }
//
// [HarmonyPatch(typeof(CharaSelectManager), nameof(CharaSelectManager.GetAssetImgColorCostumes), new Type[] {typeof(int), typeof(int), typeof(int)})]
// public static class CharaSelectManager_GetAssetImgColorCostumes_Int
// {
//     private static void Prefix(int iIcon, int iColor, int iCostume)
//     {
//         Plugin.Log.LogInfo($"CharaSelectManager_GetAssetImgColorCostumes_Int: {iIcon}, {iColor}, {iCostume}");
//     }
//
//     private static void Postfix(int iIcon, int iColor, int iCostume, Sprite __result)
//     {
//         Plugin.Log.LogInfo($"CharaSelectManager_GetAssetImgColorCostumes_Int: {iIcon}, {iColor}, {iCostume}, {__result.name}");
//     }
// }
//
// [HarmonyPatch(typeof(PlayableCharacterBuilder))]
// [HarmonyPatch(nameof(PlayableCharacterBuilder.CreatePlayableCharacter))]
// public static class PlayableCharacterBuilderCreatePlayableCharacter
// {
//     public static void Prefix(PlayableCharacterBuilder __instance)
//     {
//         __instance._colorId = 1;
//         __instance._costumeId = 1;
//         __instance._eftColorId = "c01";
//         __instance._eftCostumeId = "d01";
//         Plugin.Log.LogInfo("--- PlayableCharacterBuilder ---");
//         Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance.name = {__instance.name}");
//         Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance._costumeId = {__instance._costumeId}");
//         Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance._colorId = {__instance._colorId}");
//         Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance.eftCostume = {__instance._eftCostumeId}");
//         Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance.eftColor = {__instance._eftColorId}");
//         Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance.eftColor = {__instance._eftColorId}");
//     }
//     // public static void Postfix(PlayableCharacterBuilder __instance)
//     // {
//     //     Plugin.Log.LogInfo("--- POST ---");
//     //     Plugin.Log.LogInfo($"PlayableCharacterBuilder: __instance._colorId = {__instance._colorId}");
//     // }
// }
//
// // [HarmonyPatch(typeof(CharacterModelSystem))]
// // [HarmonyPatch(nameof(CharacterModelSystem.SetFormModel))]
// // public static class CharacterModelSystemSetFormModel
// // {
// //     public static void Prefix(
// //         int index,
// //         GameObject formModel,
// //         MeshCollections meshs,
// //         MaterialCollections materials,
// //         AddressablesHandleList materialHandles)
// //     {
// //         // Plugin.Log.LogInfo($"--- CharacterModelSystem ---");
// //         // Plugin.Log.LogInfo($"CharacterModelSystem: formModel.name = {formModel.name} | index = {index}");
// //     }
// // }
// //
//
// [HarmonyPatch(typeof(CharaSelectBlueprint))]
// [HarmonyPatch(nameof(CharaSelectBlueprint.IsConflictColor))]
// public static class CharaSelectBlueprintIsConflictColor
// {
//     public static void Postfix(
//         CharacterTeam team,
//         string characterId,
//         int color,
//         int costume
//     )
//     {
//         Plugin.Log.LogInfo($"--- CharaSelectBlueprintIsConflictColor ---");
//         Plugin.Log.LogInfo($"STACK TRACE: \n{Environment.StackTrace}");
//     }
// }
//
// [HarmonyPatch(typeof(BattleSystemBlueprint))]
// [HarmonyPatch(nameof(BattleSystemBlueprint.GetMemberId))]
// public static class CharaSelectBlueprintGetMemberId
// {
//     public static void Postfix( CharacterTeam team, int index )
//     {
//         Plugin.Log.LogInfo($"--- CharaSelectBlueprintGetMemberId ---");
//         Plugin.Log.LogInfo($"STACK TRACE: \n{Environment.StackTrace}");
//     }
// }
