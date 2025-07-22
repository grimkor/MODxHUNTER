using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using modxhunter.modules.ColliderViewer;
using UnityEngine;

namespace modxhunter.modules.OverlayMenu;

public class MenuGreenScreen : WindowBase
{
    protected override Windows WindowTypeID => Windows.GreenScreen;
    public override string Name => "GreenScreen";

    private Vector2 _scrollPosition = Vector2.zero;
    public int WindowID => 1;
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

            _window = GUILayout.Window(WindowID, _window, (GUI.WindowFunction)DrawWindow, "Green Screen Settings", _layoutOptions);
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
            MenuManager.Instance.CloseWindow(Windows.GreenScreen);
        }
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Space(25);
        GUI.enabled = true;
        GUILayout.Label("Green Screen Color", new GUIStyle(new GUIStyle(GUI.skin.label) {fontSize = 18, fontStyle = FontStyle.Bold}));
        
        int currentColorIndex = GreenScreen.Instance.GetCurrentColor();
        string[] colorNames = Enum.GetNames(typeof(GreenScreenColor));
        int newColorIndex = GUILayout.SelectionGrid(currentColorIndex, colorNames, 1);

        if (currentColorIndex != newColorIndex)
        {
            GreenScreen.Instance.SetColor(newColorIndex);
        }

        GUI.enabled = true;
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    public void Hide()
    {
        _showMenu = false;
    }
}
