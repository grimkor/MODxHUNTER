using System.IO;
using BepInEx;
using Henley.Scene;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using modxhunter.modules.OverlayMenu;
using UnityEngine;

namespace modxhunter.modules.TextureLoader;

public class MenuTextureLoader: WindowBase
{
    public override int WindowID => 3;
    private Vector2 _scrollPosition = Vector2.zero;
    private readonly Il2CppReferenceArray<GUILayoutOption> _layoutOptions = new(3);
    private Texture2D? _opaqueTexture;

    public override void Init()
    {
        base.Init();
        _layoutOptions[0] = GUILayout.Width(300);
        _layoutOptions[1] = GUILayout.Height(400);
        _layoutOptions[2] = GUILayout.ExpandHeight(true);
    }

    public void Toggle()
    {
        _showMenu = !_showMenu;
    }

    void OnGUI()
    {
        if (_showMenu)
        {
            if (_opaqueTexture == null) {
                _opaqueTexture = new Texture2D(1, 1);
                _opaqueTexture.SetPixel(0, 0, Color.black);
                _opaqueTexture.Apply();
            }

            _window = GUILayout.Window(WindowID, _window, (GUI.WindowFunction)DrawWindow, $"Texture Loader", _layoutOptions);
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }

    void DrawWindow(int windowID)
    {
        GUI.enabled = true;
        if (GUI.Button(new Rect(_window.width - 25, 5, 20, 20), "×"))
        {
            MenuManager.Instance.CloseWindow(Windows.Textures);
        }
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Space(25);
        GUI.enabled = true;
        var enableTextures = GUILayout.Toggle(TextureManager.Instance.IsEnabled, " Enabled");
        if (enableTextures != TextureManager.Instance.IsEnabled)
        {
            TextureManager.Instance.SetEnabled(enableTextures);
        }

        GUILayout.Space(10);

        GUI.enabled = SceneStartupManager.Instance._currentSceneId == "Battle2D";
        if (GUILayout.Button("Export Character Textures"))
        {
            TextureManager.ExportTextures();
        }

        GUI.enabled = true;
        GUILayout.TextArea($"""
                            Texture files will export to:
                            {Path.Join(Paths.PluginPath, "exports")}
                            """);
        GUILayout.Space(10);
        GUI.enabled = SceneStartupManager.Instance._currentSceneId == "Battle2D";
        if (GUILayout.Button("Reload From Disk"))
        {
            TextureManager.Instance.ReloadTexturesFromDisk();
        }
        GUI.enabled = true;
        GUILayout.TextArea($"""
                            Texture files are read from:
                            {Path.Join(Paths.PluginPath, "textures")}
                            *** Use the same directory layout as the exported files! ***
                            """);
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    public void Hide()
    {
        _showMenu = false;
    }
}
