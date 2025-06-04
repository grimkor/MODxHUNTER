using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using grimbahack_hxh.modules;
using HarmonyLib;
using Henley.Battle;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace grimbahack_hxh;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        var harmony = new Harmony("Base.Grimbakor.Mod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        ClassInjector.RegisterTypeInIl2Cpp<ColliderViewer.ColliderViewerBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp<KeyStrokeListener>();
        var go = new GameObject() { name = "KeyStrokeListener" };
        go.hideFlags = HideFlags.HideAndDontSave;
        Object.DontDestroyOnLoad(go);
        var ksl = go.AddComponent<KeyStrokeListener>();
        ksl.enabled = true;
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    [HarmonyPatch(typeof(BattleEventManager), nameof(BattleEventManager.BattleStartReadyInit))]
    public class BattleStartReadyInit
    {
        public static void Postfix()
        {
            Log.LogInfo("BattleStartReadyInit");
        }
    }

    [HarmonyPatch(typeof(BattleEventManager), nameof(BattleEventManager.BattleStartGoInit))]
    public class BattleStartGoInit
    {
        public static void Postfix()
        {
            Log.LogInfo("BattleStartGoInit");
        }
    }

    // [HarmonyPatch(typeof(CharacterCollisionSystem), nameof(CharacterCollisionSystem.CheckCollision))]
    // public class CheckCollision
    // {
    //     private static GameObject gameObject;
    //     private static Camera camera;
    //     private static SpriteRenderer spriteRenderer;
    //     private static RectTransform rectTransform;
    //
    //     private static void Init()
    //     {
    //         if (!gameObject)
    //         {
    //             // ClassInjector.RegisterTypeInIl2Cpp<DrawAttack>();
    //             // DrawAttachBehaviour = gameObject.AddComponent<DrawAttack>();
    //             // DrawAttachBehaviour.enabled = true;
    //             camera = BattleSystemManager.Instance._cameraSystem._camera;
    //             var cameraObject = camera.transform.gameObject;
    //             gameObject = new();
    //             Object.DontDestroyOnLoad(gameObject);
    //             gameObject.hideFlags = HideFlags.HideAndDontSave;
    //             gameObject.transform.SetParent(cameraObject.transform);
    //             rectTransform = gameObject.AddComponent<RectTransform>();
    //             spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    //             var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(1, 1, 1, 1), Vector2.one);
    //             spriteRenderer.sprite = sprite;
    //             gameObject.transform.SetPositionAndRotation(new(0,0 ,5), new());
    //         }
    //     }
    //
    //     public static void Postfix(
    //         CharacterCore attacker,
    //         PlayableCharacterCore damager,
    //         FrameHitBox targetCols,
    //         FrameHitBox attackCols,
    //         FrameHitBox throwCols,
    //         bool isGuard,
    //         ref Vector2Int hitResult
    //     )
    //     {
    //         Log.LogInfo("Check enabled");
    //         if (!gameObject)
    //         {
    //             Log.LogInfo("Not enabled, enabling");
    //             Init();
    //         }
    //
    //         var vec = Vector2.zero;
    //         var color = Color.red;
    //         Log.LogInfo("---- START ----");
    //         Log.LogInfo(attacker._characterId);
    //         Log.LogInfo(targetCols);
    //         Log.LogInfo(attackCols);
    //         Log.LogInfo(throwCols);
    //         Log.LogInfo(isGuard);
    //         Log.LogInfo(hitResult);
    //         Log.LogInfo("---- END ----");
    //
    //         foreach (var collider in attackCols._colliderArray)
    //         {
    //             rectTransform.sizeDelta = collider._size;
    //
    //             // collider.ConvertDraw(ref vec, true, out var p, out var s);
    //             // BattleSystemManager.Instance.IsDrawGizmos = true;
    //             // BattleSystemManager.Instance.DrawGizmoMode = BattleSystemManager.GizmoMode.Attack;
    //             // collider.DrawGizmos(ref color, ref vec, true, true, false);
    //             // rectTransform.localPosition = new(p.x, p.y, 6);
    //         }
    //     }
    // }

    [HarmonyPatch(typeof(CharacterCollisionSystem), nameof(CharacterCollisionSystem.DrawGizumosHitBox))]
    public class DrawGizumosHitBox
    {
        public static void Postfix()
        {
            Log.LogInfo("DrawGizumosHitBox");
        }
    }

    [HarmonyPatch(typeof(CharacterCollisionSystem), nameof(CharacterCollisionSystem.DrawGizumos))]
    public class DrawGizumos
    {
        public static void Postfix()
        {
            Log.LogInfo("DrawGizumos");
        }
    }

    public class DrawAttack : MonoBehaviour
    {
        private Rect _rect;
        private Rect _rectTest;
        private readonly Texture2D _text = new(1,1);

        private void Start()
        {
            _text.SetPixel(1,1,Color.red);
            _text.wrapMode = TextureWrapMode.Repeat;
            _text.Apply();
            _rectTest = new(200,200,200,200);
        }

        public void UpdatePosition(Vector2 position, Vector2 size)
        {
            // var camera = BattleSystemManager.Instance._cameraSystem._camera;
            // var pos = camera.WorldToScreenPoint(characterPosition);
            // pos.y = Screen.currentResolution.height - pos.y;
            _rect = new Rect(position, size);
            Log.LogInfo($"pos: {position.x}, {position.y} | size: {size.x}, {size.y}");
        }

        private void OnGUI()
        {
            GUI.DrawTexture(_rect, _text);
            GUI.DrawTexture(_rectTest, _text);
            Gizmos.DrawCube(_rect.position, _rect.size);
        }
    }

}

