using Henley.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace modxhunter.modules;

public class KeyStrokeListener : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            if (SceneStartupManager.Instance._currentSceneId == "Battle2D")
            {
                ColliderViewer.ColliderViewer.Instance.Toggle();
            }
        }
    }
}
