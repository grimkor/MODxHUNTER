using System;
using System.Collections.Generic;
using System.Linq;
using modxhunter.modules.PostProcessing;
using UnityEngine;
using Object = UnityEngine.Object;

namespace modxhunter.modules.OverlayMenu;

public class MenuManager
{
    public static MenuManager Instance = new();
    private MenuManagerBehaviour? _behaviour;
    private Dictionary<Windows, WindowBase> _windowIds = new();
    private List<Windows> _openWindows = new();
    private GameObject _gameObject;
    public bool IsOpen => Instance._windowIds.Any(w => w.Value.IsActive);

    public void Init()
    {
        if (_behaviour == null)
        {
            _gameObject = new GameObject() {name = "mxh-overlay-menu-behaviour"};
            _gameObject.hideFlags = HideFlags.HideAndDontSave;
            _behaviour = _gameObject.AddComponent<MenuManagerBehaviour>();
            Object.DontDestroyOnLoad(_gameObject);
        }
    }

    public void RegisterWindow(Windows id, WindowBase window)
    {
        Instance._windowIds.TryAdd(id, window);
    }

    private WindowBase CreateWindow(Windows id)
    {
        switch (id)
        {
            case Windows.MainMenu:
                return Instance._gameObject.AddComponent<MenuOverlay>();
            case Windows.PostProcessing:
                return _gameObject.AddComponent<MenuPostProcessing>();
            case Windows.GreenScreen:
                return _gameObject.AddComponent<MenuGreenScreen>();
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }
    }

    public void CloseWindow(Windows id)
    {
        if (id == Windows.MainMenu) return;
        Instance._windowIds[id]?.SetActive(false);
        Object.DestroyImmediate(Instance._windowIds[id]);
        Instance._openWindows.Remove(id);
        Instance._windowIds.Remove(id);
    }
    public void OpenWindow(Windows id)
    {
        if (Instance._openWindows.Contains(id)) return;
        var window = Instance.CreateWindow(id);
        window.Init();
        Instance.RegisterWindow(id, window);
        window.SetPosition(new(40 * Instance._openWindows.Count + 1, 40 * Instance._openWindows.Count + 1));
        window.SetActive(true);
        if (Instance._openWindows.Contains(id)) return;
        Instance._openWindows.Add(id);
    }

    public void CloseLastOpenWindow()
    {
        if (!Instance._openWindows.Any()) return;
        var lastWindow = Instance._openWindows.Last();
        CloseWindow(lastWindow);
    }



    public void Toggle(Windows id)
    {
        var open = Instance.GetWindow(Windows.MainMenu).IsActive;
        if (open)
        {
            Instance.OpenWindow(id);
        }
        else
        {
            Instance.CloseWindow(id);
        }
    }

    public void ToggleAll()
    {
        if (Instance._openWindows.Any())
        {
            var open = Instance.GetWindow(Windows.MainMenu).IsActive;
            Instance._openWindows.ForEach(w =>
            {
                Instance.GetWindow(w)?.SetActive(!open);
                GUI.BringWindowToFront(Instance.GetWindow(w).WindowID);
            });
        }
        else
        {
            Instance.OpenWindow(Windows.MainMenu);
        }
    }

    private WindowBase GetWindow(Windows windows)
    {
        var w = Instance._windowIds[windows];
        if (w == null)
        {
            throw new Exception($"Window {windows} not found");
        }
        return w;
    }

    public class MenuManagerBehaviour : MonoBehaviour
    {
        private Texture2D? _opaqueTexture;
        private GUIStyle? _originalSliderStyle;

        void OnGUI()
        {
            if (_originalSliderStyle == null)
            {
                _originalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            }
            if (_opaqueTexture == null) {
                _opaqueTexture = new Texture2D(1, 1);
                _opaqueTexture.SetPixel(0, 0, Color.black);
                _opaqueTexture.Apply();
            }
            GUI.skin.window.normal.background = _opaqueTexture;
            GUI.skin.window.onNormal.background = _opaqueTexture;
            GUI.skin.window.active.background = _opaqueTexture;
            GUI.skin.window.onActive.background = _opaqueTexture;
            GUI.skin.window.focused.background = _opaqueTexture;
            GUI.skin.window.onFocused.background = _opaqueTexture;
            GUI.skin.window.border.bottom = 1;
            GUI.skin.window.border.top = 1;
            GUI.skin.window.border.left = 1;
            GUI.skin.window.border.right = 1;
            GUI.skin.horizontalSlider = new GUIStyle(_originalSliderStyle);
            GUI.skin.horizontalSlider.normal.background = Texture2D.grayTexture;
            GUI.skin.window.fontSize = 16;
            GUI.skin.button.fontSize = 16;
            GUI.skin.horizontalSlider.fontSize = 16;
            GUI.skin.toggle.fontSize = 16;
            GUI.skin.label.fontSize = 16;
        }
    }
}

public enum Windows
{
    MainMenu,
    PostProcessing,
    GreenScreen
}
