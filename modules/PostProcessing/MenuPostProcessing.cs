using System;
using Henley.ECP.Volume;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using modxhunter.modules.OverlayMenu;
using UnityEngine;
using UnityEngine.Rendering;

namespace modxhunter.modules.PostProcessing;

public class MenuPostProcessing: WindowBase
{
    public override int WindowID => 2;
    private Vector2 _scrollPosition = Vector2.zero;
    private readonly Il2CppReferenceArray<GUILayoutOption> _layoutOptions = new(3);
    private Texture2D? _opaqueTexture;
    private float diffusionValue = 0.45f;

    public override void Init()
    {
        base.Init();
        _layoutOptions[0] = GUILayout.Width(300);
        _layoutOptions[1] = GUILayout.Height(400);
        _layoutOptions[2] = GUILayout.ExpandHeight(true);
        // _window.x = 340;
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

            _window = GUILayout.Window(WindowID, _window, (GUI.WindowFunction)DrawWindow, $"Post Processing", _layoutOptions);
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }

    void DrawWindow(int windowID)
    {
        if (GUI.Button(new Rect(_window.width - 25, 5, 20, 20), "×"))
        {
            MenuManager.Instance.CloseWindow(Windows.PostProcessing);
        }
        diffusionValue = PostProcessing.Instance.DiffusionValue;
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Space(25);
        GUI.enabled = true;
        GUILayout.Label("Diffusion", new GUIStyle(new GUIStyle(GUI.skin.label) {fontSize = 18, fontStyle = FontStyle.Bold}));
        var enableDiffusion = GUILayout.Toggle(PostProcessing.Instance.DiffusionEnabled, " Enable");
        if (enableDiffusion != PostProcessing.Instance.DiffusionEnabled)
        {
            PostProcessing.Instance.EnableDiffusion(enableDiffusion);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label($"({diffusionValue:F2})", GUILayout.Width(50));;
        GUILayout.BeginVertical();
        GUILayout.Space(12);
        diffusionValue = (float)Math.Round(GUILayout.HorizontalSlider(diffusionValue, 0f, 1f), 2);
        if (!Mathf.Approximately(diffusionValue, PostProcessing.Instance.DiffusionValue))
        {
            PostProcessing.Instance.SetDiffusionValue(diffusionValue);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Anti-Aliasing", new GUIStyle(new GUIStyle(GUI.skin.label) {fontSize = 18, fontStyle = FontStyle.Bold}));
        GUILayout.BeginHorizontal();
        int currentAAIndex = (int)PostProcessing.Instance.Antialiasing;
        string[] aaOptions = new[] { "Fast", "Subpixel" };
        int newAAIndex = GUILayout.SelectionGrid(currentAAIndex, aaOptions, 2);
        if (currentAAIndex != newAAIndex)
        {
            PostProcessing.Instance.SetAntiAliasing((AntialiasingType)newAAIndex);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Bloom", new GUIStyle(new GUIStyle(GUI.skin.label) {fontSize = 18, fontStyle = FontStyle.Bold}));
        var enableBloom = GUILayout.Toggle(PostProcessing.Instance.Bloom, "Enable Bloom");
        if (enableBloom != PostProcessing.Instance.Bloom)
        {
            PostProcessing.Instance.SetBloom(enableBloom);
        }

        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    public void Hide()
    {
        _showMenu = false;
    }
}
