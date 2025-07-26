using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace modxhunter.modules.ColliderViewer;

public class GreenScreen
{
    private GameObject? _bgSprite;
    private GameObject? _background;
    public static readonly GreenScreen Instance = new();
    private GreenScreenBehaviour? _behaviour;

    public static void Init()
    {
        if (!Instance._behaviour)
        {
            var go = new GameObject() {name = "mxh-green-screen-behaviour"};
            go.hideFlags = HideFlags.HideAndDontSave;
            Instance._behaviour = go.AddComponent<GreenScreenBehaviour>();
            Object.DontDestroyOnLoad(go);
            Instance._behaviour.enabled = false;
        }
        if (Instance._background == null)
        {
            Instance._background = GameObject.Find("BackgroundRoot");
        }
        if (Instance._bgSprite == null)
        {
            Instance._createBG();
        }
    }

    public void Toggle()
    {
        Init();
        if (Instance.IsEnabled())
        {
            Instance.SetBehaviour(false);
            Instance.ShowGreenScreen(false);
            if (Instance._bgSprite == null)
            {
                Instance._createBG();
            }
        }
        else
        {
            Instance.SetBehaviour(true);
            Instance.ShowGreenScreen(true);
        }
    }

    private void ShowGreenScreen(bool enabled)
    {
        if (Instance._background == null) return;
        Instance._background.active = !enabled;
        Instance._bgSprite?.SetActive(enabled);
    }

    private void SetBehaviour(bool enabled)
    {
        if (Instance._behaviour == null) return;
        Instance._behaviour.enabled = enabled;
    }

    public bool IsEnabled()
    {
        if (Instance._behaviour == null) return false;
        return Instance._behaviour.enabled;
    }

    private void _createBG()
    {
        if (_bgSprite != null) return;
        var tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        var color = new Color(1.0f, 1.0f, 1.0f, 1f);
        var fillColorArray = Enumerable.Repeat(color, 512 * 512).ToArray();
        tex.SetPixels(fillColorArray);
        tex.Apply();
        var sprite = Sprite.Create(tex, new(1, 1, 256, 256), Vector2.one, 128, UInt32.MinValue, SpriteMeshType.FullRect);
        var go = new GameObject("BackgroundGo");
        go.hideFlags = HideFlags.HideAndDontSave;
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(512, 512);
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Instance.ToColor(Instance._colorIndex);
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        // spriteRenderer.sortingOrder = 10000;
        spriteRenderer.sortingLayerName = "Background";
        go.transform.localScale = new(100, 100, 1);
        go.transform.position = new(50, 50, -1.5f);
        _bgSprite = go;
        _bgSprite.SetActive(false);
    }

    public class GreenScreenBehaviour : MonoBehaviour
    {
        void Update()
        {
                Instance.ShowGreenScreen(enabled);
        }
    }

    private int _colorIndex = 0;

    public int GetCurrentColor()
    {
        return Instance._colorIndex;
    }

    public void SetColor(int index)
    {
        if (index >= 0 && index < 8)
        {
            Instance._colorIndex = index;
            if (Instance._bgSprite != null)
            {
                Instance._bgSprite.GetComponent<SpriteRenderer>().color = Instance.ToColor(index);
            }
        }
    }

    public Color ToColor(int index)
    {
        switch (index)
        {
            case 0:
                return Color.black;
            case 1:
                return Color.white;
            case 2:
                return Color.red;
            case 3:
                return Color.green;
            case 4:
                return Color.blue;
            case 5:
                return Color.yellow;
            case 6:
                return Color.cyan;
            case 7:
                return Color.magenta;
            default:
                return Color.black;
        };
    }

}

public enum GreenScreenColor
{
    Black,
    White,
    Red,
    Green,
    Blue,
    Yellow,
    Cyan,
    Magenta
}
