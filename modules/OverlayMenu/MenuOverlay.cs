using System.Reflection;
using Henley;
using Henley.Scene;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using modxhunter.modules.ColliderViewer;
using UnityEngine;

namespace modxhunter.modules.OverlayMenu;

public class MenuOverlay : WindowBase
{
    protected override Windows WindowTypeID => Windows.MainMenu;
    public override string Name => "MainMenu";

    private Vector2 _scrollPosition = Vector2.zero;
    private readonly Il2CppReferenceArray<GUILayoutOption> _layoutOptions = new(3);
    private Texture2D? _opaqueTexture;
    private GUIStyle? _originalSliderStyle;
    public override int WindowID => 0;
    private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "???";


    public override void Init()
    {
        base.Init();
        _layoutOptions[0] = GUILayout.Width(300);
        _layoutOptions[1] = GUILayout.Height(400);
        _layoutOptions[2] = GUILayout.ExpandHeight(true);
    }

    void OnGUI()
    {
        if (_showMenu)
        {
            _window = GUILayout.Window(WindowID, _window, (GUI.WindowFunction)DrawWindow, $"MODxHUNTER ({Version})", _layoutOptions);
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }

    void DrawWindow(int windowID)
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        GUI.enabled = SceneStartupManager.Instance._currentSceneId == "Battle2D";
        if (GUILayout.Button(ColliderViewer.ColliderViewer.Instance.IsEnabled() ? "Disable Collider Viewer" : "Enable Collider Viewer"))
        {
            ColliderViewer.ColliderViewer.Instance.Toggle();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(GreenScreen.Instance.IsEnabled() ? "Disable Green Screen" : "Enable Green Screen"))
        {
            GreenScreen.Instance.Toggle();
        }
        GUI.enabled = true;
        if (GUILayout.Button("...", GUILayout.Width(25)))
        {
            MenuManager.Instance.Toggle(Windows.GreenScreen);
        }
        GUILayout.EndHorizontal();


        GUI.enabled = SceneStartupManager.Instance._currentSceneId == "Battle2D";
        GUI.enabled = true;
        if (GUILayout.Button("Post-Processing"))
        {
            MenuManager.Instance.Toggle(Windows.PostProcessing);
        }
        if (GUILayout.Button("Custom Textures"))
        {
            MenuManager.Instance.Toggle(Windows.Textures);
        }
        GUI.enabled = true;
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

}
