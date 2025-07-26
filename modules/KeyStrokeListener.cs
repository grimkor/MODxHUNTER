using System;
using modxhunter.modules.OverlayMenu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UIElements;

namespace modxhunter.modules;

public class KeyStrokeListener : MonoBehaviour
{
    public static void Init()
    {
        var go = new GameObject() { name = "mxh-KeyStrokeListener" };
        go.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(go);
        var ksl = go.AddComponent<KeyStrokeListener>();
        ksl.enabled = true;
    }
    // private bool _cloned;
    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            MenuManager.Instance.ToggleAll();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (MenuManager.Instance.IsOpen)
                MenuManager.Instance.CloseLastOpenWindow();
        }

        // if (Keyboard.current.f5Key.wasPressedThisFrame)
        // {
        //     if (!_cloned)
        //     {
        //         var gon = CharacterSystem.Instance._charList.GetRowDataKey("p01a");
        //         if (gon == null) return;
        //         var costumes = gon.CostumeColors;
        //         var clone = new Il2CppReferenceArray<CharacterListDataParam.CostumeColor>(new []{costumes[0], costumes[1], costumes[0]});
        //         gon.CostumeColors = clone;
        //         var variations = costumes[0].variation.ToList();
        //         variations.Add(Color.magenta);
        //         var locked = costumes[0].LockedColorVariation.ToList();
        //         locked.Add(false);
        //         gon.CostumeColors[0].variation = variations.ToArray();
        //         gon.CostumeColors[0].LockedColorVariation = locked.ToArray();
        //
        //
        //         var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
        //         Plugin.Log.LogInfo($"TextureToCopy: {textures.Length}");
        //         var textureList = textures.ToList();
        //         Plugin.Log.LogInfo($"TextureToCopy: Finding");
        //         foreach (var tex in textures)
        //         {
        //             if (!tex.name.Contains("p01a")) continue;
        //             Plugin.Log.LogInfo($"TextureToCopy: {tex.name}");
        //         }
        //         var textureToCopy = textureList.First(s => s.name == "T_ui_CharacterSelect_Img_p01a_d01_c01_D");
        //         Plugin.Log.LogInfo($"TextureToCopy: {textureToCopy.name}");
        //         if (textureToCopy)
        //         {
        //             // var copy = new Texture2D(2, 2);
        //             // ImageConversion.LoadImage(copy, ImageConversion.EncodeToPNG(textureToCopy));
        //             // copy.name = "T_ui_CharacterSelect_Img_p01a_d01_c05_D";
        //             // T_chr_p01a_w01_c01_Base
        //         }
        //         _cloned = true;
        //     }
        // }
    }
}
