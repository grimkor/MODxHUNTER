using modxhunter.modules.OverlayMenu;
using UnityEngine;
using UnityEngine.InputSystem;

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
    }
}
