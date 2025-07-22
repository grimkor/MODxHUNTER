using UnityEngine;

namespace modxhunter.modules.OverlayMenu;

public abstract class WindowBase : MonoBehaviour
{
    protected virtual Windows WindowTypeID { get; }
    protected bool _showMenu;
    protected Rect _window = new(20, 20, 300, 400);
    public virtual string Name { get; private set; }
    public virtual int WindowID { get; private set; }
    public bool IsActive => _showMenu;

    // public void Init<T>(Windows windowTypeType, string goName) where T : WindowBase
    public virtual void Init()
    {
        // var go = new GameObject
        // {
        //     name = goName,
        //     hideFlags = HideFlags.HideAndDontSave
        // };
        // DontDestroyOnLoad(go);
        // go.AddComponent<T>();
        // MenuManager.Instance.RegisterWindow(windowTypeType);
    }
    public virtual void SetActive(bool active)
    {
        _showMenu = active;
    }

    public void SetPosition(Vector2 position)
    {
        _window.x = position.x;
        _window.y = position.y;
    }
}
