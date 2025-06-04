using System;
using System.Collections.Generic;
using System.Linq;
using ECC;
using Henley.Battle;
using Il2CppInterop.Runtime.Injection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace grimbahack_hxh.modules;

public  class ColliderViewer
{
    public static readonly ColliderViewer Instance = new();
    private List<PlayableCharacterCore> _playableCharacters = new();
    private List<GameObject> _overlayGameObjects = new();
    private ColliderViewerBehaviour _behaviour;
    private static Dictionary<HitBoxType, Color> _colors = new()
    {
        {HitBoxType.Damage, new Color(0, 1, 0, 0.6f)},
        {HitBoxType.Attack, new Color(1, 0, 0, 0.6f)}
    };

    public void Init()
    {
        Plugin.Log.LogInfo("Running Init");
        var bsm = BattleSystemManager.Instance;
        var teamOne = bsm.Team.GetTeamEntry(CharacterTeam.Alpha);
        var teamTwo = bsm.Team.GetTeamEntry(CharacterTeam.Bravo);
        var createObjects = false;
        if (!Instance._behaviour)
        {
            createObjects = true;
            Instance._overlayGameObjects.Clear();
            var go = new GameObject("ColliderViewerBehaviour")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            Object.DontDestroyOnLoad(go);
            Instance._behaviour = go.AddComponent<ColliderViewerBehaviour>();
        }
        Instance._playableCharacters.Clear();
        for (int i = 0; i < 3; i++)
        {
            var member = teamOne.GetMember(i);
            Instance._playableCharacters.Add(member);
            if (createObjects) Instance.GenerateOverlayGameObject(i, member);
        }
        for (int i = 0; i < 3; i++)
        {
            var member = teamTwo.GetMember(i);
            Instance._playableCharacters.Add(member);
            if (createObjects) Instance.GenerateOverlayGameObject(i + 3, member);
        }

        Instance._behaviour.enabled = true;
    }

    private void GenerateOverlayGameObject(int position, PlayableCharacterCore member)
    {
            var go = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = $"overlay{position}",
                active = false,
            };
            Object.DontDestroyOnLoad(go);
            Instance._overlayGameObjects.Add(go);
            for (int i = 0; i < member.Collision.GetHitBox(HitBoxType.Damage, member._actionName, 0).ColliderCount; i++)
            {
                var overlayObj = new GameObject();
                overlayObj.name = $"overlay{position}-box{i}";
                overlayObj.SetParent(go);
                overlayObj.AddComponent<RectTransform>();
                var sprite = overlayObj.AddComponent<SpriteRenderer>();
                sprite.sprite = Sprite.Create(Texture2D.whiteTexture, new(1,1,1,1), Vector2.one);
                sprite.size = Vector2.one;
                sprite.drawMode = SpriteDrawMode.Tiled;
                sprite.color = _colors[HitBoxType.Damage];
                overlayObj.transform.localScale = Vector3.one;
            }
    }

    private bool IsOnScreen(int position)
    {
        if (position >= Instance._playableCharacters.Count)
        {
            // Plugin.Log.LogInfo($"IsOnScren: {position} False");
            return false;
        }
        // Plugin.Log.LogInfo($"IsOnScren: {position} {!Instance._playableCharacters[position].IsInvincibility(AttackCategoryType.Nothing)}");
        // return !Instance._playableCharacters[position].IsInvincibility(AttackCategoryType.Nothing);
        var character = Instance._playableCharacters[position];
        return character.IsMainPlayer ||
               BattleSystemManager.Instance.Team.IsAssistAppeared(character.Team, GetMemberIdFromPosition(position));
    }

    private int GetMemberIdFromPosition(int position)
    {
        return position - (position < 3 ? 0 : 3);
    }

    private GameObject GetCharacterOverlay(int position, HitBoxType type)
    {
        if (type != HitBoxType.Damage)
        {
            return null;
        }
        return Instance._overlayGameObjects[position];
    }

    public List<Box2DCollider> GetDamageHitboxes(int position)
    {
        if (position >= Instance._playableCharacters.Count)
        {
            return null;
        }
        var player = Instance._playableCharacters[position];
        return player.Collision.GetHitBox(
            HitBoxType.Damage,
            player._actionName,
            0
        )._colliderArray.ToList();
    }

    public void DrawColliders(int position, List<Box2DCollider> colliders)
    {
        if (position >= _playableCharacters.Count)
        {
            return;
        }
        var overlayObject = GetCharacterOverlay(position, HitBoxType.Damage);
        var character = _playableCharacters[position];
        if (!overlayObject || character == null) return;
        var mod = character.Direction == ObjectDirection.Right ? 1 : -1;
        for (int i = 0; i < colliders.Count; i++)
        {
            var box = overlayObject.GetChild(i);
            var sprite = box.GetComponent<SpriteRenderer>();
            var collider = colliders[i];
            var sizeDelta = new Vector2(mod * (float)collider._size.x / 1000, (float)collider._size.y / 1000);
            sprite.size = sizeDelta;
            box.transform.position = new Vector3(
                (float)character.PositionX / 1000
                 + mod * (float)collider._position.x / 1000
                 + sizeDelta.x,
                ((float)character.PositionY / 1000) + (float)collider._position.y / 1000 + sizeDelta.y,
                0
            );
            box.SetActive(true);
        }
        if (colliders.Count < overlayObject.transform.GetChildCount())
        {
            for (int childIndex = colliders.Count; childIndex < overlayObject.transform.GetChildCount(); childIndex++)
            {
                var box = overlayObject.transform.GetChild(childIndex);
                box.gameObject.SetActive(false);
            }
        }
    }

    public class ColliderViewerBehaviour : MonoBehaviour
    {
        private void Update()
        {
            for (int i = 0; i < 6; i++)
            {
                if (Instance.IsOnScreen(i))
                {
                    Instance._overlayGameObjects[i].active = true;
                    var boxes = Instance.GetDamageHitboxes(i);
                    Instance.DrawColliders(i, boxes);
                }
                else
                {
                    Instance._overlayGameObjects[i].active = false;
                }
            }
        }
    }

    public void Disable()
    {
        if (Instance._behaviour)
        {
            foreach (var go in Instance._overlayGameObjects)
            {
                go.active = false;
            }
            Instance._behaviour.enabled = false;
        }
    }
}

public class KeyStrokeListener : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Plugin.Log.LogInfo("Debug Pressed");
            ColliderViewer.Instance.Init();
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            Plugin.Log.LogInfo("Debug Pressed");
            ColliderViewer.Instance.Disable();
        }
    }
}
