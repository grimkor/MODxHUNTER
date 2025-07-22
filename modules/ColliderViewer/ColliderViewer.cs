using System.Collections.Generic;
using System.Linq;
using ECC;
using HarmonyLib;
using Henley.Battle;
using Henley.Scene;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace modxhunter.modules.ColliderViewer;

public  class ColliderViewer
{
    public static readonly ColliderViewer Instance = new();
    private List<PlayableCharacterCore> _playableCharacters = new();
    private List<CharacterOverlay> _characterOverlays = new();
    private List<SubCharacterOverlay> _subCharacters = new();
    private ColliderViewerBehaviour? _behaviour;
    private bool _enabled;

    private bool Enabled
    {
        get => Instance._enabled;
        set
        {
            Instance._enabled = value;
            if (value)
            {
                Instance.Init();
            }
            else
            {
                Instance.Cleanup();
            }
        }
    }

    public void Toggle()
    {
        Instance.Enabled = !Instance._enabled;
    }

    public void Init()
    {
        Instance.Cleanup();
        Plugin.Log.LogInfo("Initialising Collider Viewer.");
        var bsm = BattleSystemManager.Instance;
        var teamOne = bsm.Team.GetTeamEntry(CharacterTeam.Alpha);
        var teamTwo = bsm.Team.GetTeamEntry(CharacterTeam.Bravo);
        var createObjects = false;
        if (Instance._behaviour == null)
        {
            createObjects = true;
            Instance._characterOverlays.ForEach(overlay =>
            {
                overlay.ParentGameObject.Destroy();
            });
            Instance._characterOverlays.Clear();
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
            foreach (var sub in member.SubCharaSystem.SubCharas.Items)
            {
                Instance._subCharacters.Add(new SubCharacterOverlay(sub));
            }
            if (createObjects) Instance.GenerateOverlayGameObject(i, member);
        }
        for (int i = 0; i < 3; i++)
        {
            var member = teamTwo.GetMember(i);
            Instance._playableCharacters.Add(member);
            foreach (var sub in member.SubCharaSystem.SubCharas.Items)
            {
                Instance._subCharacters.Add(new SubCharacterOverlay(sub));
            }
            if (createObjects) Instance.GenerateOverlayGameObject(i + 3, member);
        }
        Instance._behaviour.enabled = true;
    }


    private void GenerateOverlayGameObject(int _, PlayableCharacterCore member)
    {
        var overlay = new CharacterOverlay(member);
        Instance._characterOverlays.Add(overlay);
    }

    private bool IsOnScreen(int position)
    {
        if (position >= Instance._playableCharacters.Count)
        {
            return false;
        }
        var character = Instance._playableCharacters[position];
        return character.IsMainPlayer ||
               BattleSystemManager.Instance.Team.IsAssistAppeared(character.Team, GetMemberIdFromPosition(position));
    }

    private int GetMemberIdFromPosition(int position)
    {
        return position - (position < 3 ? 0 : 3);
    }

    private CharacterOverlay GetCharacterOverlay(int position)
    {
        return Instance._characterOverlays[position];
    }

    private List<Box2DCollider> GetDamageHitboxes(int position)
    {
        var player = Instance._playableCharacters[position];
        return player.Collision.GetHitBox(
            HitBoxType.Damage,
            player._actionName,
            player.ActionSystem.RealTimeFrame
        )._colliderArray.ToList();
    }


    private void DrawColliders(int position, List<RenderBoxData> colliders, HitBoxType type)
    {
        if (position >= _playableCharacters.Count)
        {
            return;
        }
        var overlayObject = GetCharacterOverlay(position);
        var renderTargets = overlayObject.GetRenderTargets(type);
        for (int i = 0; i < colliders.Count; i++)
        {
            Draw(renderTargets[i], colliders[i]);
        }
        if (colliders.Count < renderTargets.Count)
        {
            for (int childIndex = colliders.Count; childIndex < renderTargets.Count; childIndex++)
            {
                var box = renderTargets[childIndex];
                box.Item1.gameObject.SetActive(false);
            }
        }
    }

    private void DrawColliders(SubCharacterOverlay overlay, List<Box2DCollider> colliders, HitBoxType type)
    {
        var renderTargets = overlay.GetRenderTargets(type);
        var facingLeft = overlay.GetCharacterDirection() == ObjectDirection.Left;
        for (int i = 0; i < colliders.Count; i++)
        {
            Draw(renderTargets[i], colliders[i], overlay.GetCharacterPosition(), facingLeft);
        }
        if (colliders.Count < renderTargets.Count)
        {
            for (int childIndex = colliders.Count; childIndex < renderTargets.Count; childIndex++)
            {
                var box = renderTargets[childIndex];
                box.Item1.gameObject.SetActive(false);
            }
        }
    }
    private void DrawColliders(int position, List<Box2DCollider> colliders, HitBoxType type)
    {
        if (position >= _playableCharacters.Count)
        {
            return;
        }
        var overlayObject = GetCharacterOverlay(position);
        var renderTargets = overlayObject.GetRenderTargets(type);
        var character = _playableCharacters[position];
        var facingLeft = character.Direction == ObjectDirection.Left;
        for (int i = 0; i < colliders.Count; i++)
        {
            Draw(renderTargets[i], colliders[i], new(character.PositionX, character.PositionY), facingLeft, type == HitBoxType.Damage);
        }
        if (colliders.Count < renderTargets.Count)
        {
            for (int childIndex = colliders.Count; childIndex < renderTargets.Count; childIndex++)
            {
                var box = renderTargets[childIndex];
                box.Item1.gameObject.SetActive(false);
            }
        }
    }


    private void Draw((Transform, SpriteRenderer) renderTarget, RenderBoxData renderBoxData)
    {
        if (renderBoxData.Collider == null) return;
        var box = renderTarget.Item1;
        var sprite = renderTarget.Item2;
        var reverse = renderBoxData.DirectionFacing == ObjectDirection.Left ? -1 : 1;
        var offset = new Vector2(reverse * renderBoxData.Offset.x / 1000, renderBoxData.Offset.y / 1000);
        sprite.size = new(
            reverse * (float)renderBoxData.Collider._size.x / 1000,
            (float)renderBoxData.Collider._size.y / 1000
        );
        box.transform.position = new Vector3(
            renderBoxData.RootPosition.x / 1000
            + reverse * (float)renderBoxData.Collider._position.x / 1000
            + offset.x,
            (renderBoxData.RootPosition.y / 1000) + (float)renderBoxData.Collider._position.y / 1000 + offset.y,
            0
        );
        box.gameObject.SetActive(true);
    }

    private void Draw((Transform, SpriteRenderer) renderTargets, Box2DCollider collider, Vector2 rootPosition, bool reverse = false, bool isDamage = false)
    {
            var box = renderTargets.Item1;
            var sprite = renderTargets.Item2;
            var mod = reverse ? -1 : 1;
            var sizeDelta = new Vector2(mod * (float)collider._size.x / 1000, (float)collider._size.y / 1000);
            sprite.size = sizeDelta;
            box.transform.position = new Vector3(
                rootPosition.x / 1000
                 + mod * (float)collider._position.x / 1000
                 + sizeDelta.x,
                (rootPosition.y / 1000) + (float)collider._position.y / 1000 + sizeDelta.y,
                isDamage ? -0.1f : 0
            );
            box.gameObject.SetActive(true);
    }

    public class ColliderViewerBehaviour : MonoBehaviour
    {
        private void Update()
        {
            for (int i = 0; i < 6; i++)
            {
                if (Instance.IsOnScreen(i))
                {
                    var player = Instance._playableCharacters[i];
                    var attackBoxes = player.Collision.GetHitBox(HitBoxType.Attack, player._actionName, player.ActionSystem.RealTimeFrame);
                    var projBoxes = GetProjBoxesFromPlayer(player);
                    var attackColliders = new List<Box2DCollider>();
                    if (attackBoxes?._colliderArray != null)
                    {
                        attackColliders = attackBoxes._colliderArray.ToList();
                    }
                    var list = new List<RenderBoxData>();
                    foreach (var collider in attackColliders)
                    {
                        list.Add(new()
                        {
                            RootPosition = new(player.PositionX, player.PositionY),
                            Offset = collider._size,
                            Size = collider._size,
                            Collider = collider,
                            DirectionFacing = player.Direction,
                            Type = HitBoxType.Attack
                        });
                    }
                    for (int x = 0; x < projBoxes.Count; x++)
                    {
                        list.Add(projBoxes[x]);
                    }
                    Instance._characterOverlays[i].ParentGameObject.active = true;
                    var boxes = Instance.GetDamageHitboxes(i);
                    // Instance.DrawColliders(i, attackColliders , HitBoxType.Attack);
                    Instance.DrawColliders(i, boxes, HitBoxType.Damage);
                    Instance.DrawColliders(i, list, HitBoxType.Attack);
                }
                else
                {
                    Instance._characterOverlays[i].ParentGameObject.active = false;
                }
            }

            for (int i = 0; i < Instance._subCharacters.Count; i++)
            {
                var subCharacter = Instance._subCharacters[i];
                if (!Instance._subCharacters[i].ShouldRender())
                {
                    subCharacter.ParentGameObject.active = false;
                    continue;
                }
                subCharacter.ParentGameObject.active = true;
                var attackColliders = subCharacter.GetAttackColliders();
                Instance.DrawColliders(subCharacter, attackColliders, HitBoxType.Attack);
            }
        }

        private List<RenderBoxData> GetProjBoxesFromPlayer(PlayableCharacterCore player)
        {
            var boxList = new List<RenderBoxData>();
            if (player.ShotSystem.UsedShot.IsEmpty) return boxList;
            foreach (var shotItem in player.ShotSystem.UsedShot.Items)
            {
                if (shotItem == null) break;
                foreach (var collider in shotItem._hitCollider)
                {
                    var boxData = new RenderBoxData
                    {
                        RootPosition = shotItem._position,
                        Offset = collider._size,
                        Size = collider._size,
                        Collider = collider,
                        DirectionFacing = shotItem.Direction,
                        Type = HitBoxType.Attack,

                    };
                    if (shotItem.ShotData.SkillId == "AABShot")
                    {
                        boxData.Offset.x -= shotItem.ShotData._bonePosOffset.x;
                        boxData.Offset.y += shotItem.ShotData._bonePosOffset.y;
                        boxData.RootPosition.x = player.PositionX;
                        boxData.RootPosition.y = player.PositionY;
                    }
                    boxList.Add(boxData);
                }
            }
            return boxList;
        }
    }

    public void Disable()
    {
        if (Instance._behaviour != null)
        {
            Instance._behaviour.enabled = false;
            foreach (var go in Instance._characterOverlays)
            {
                go.ParentGameObject.active = false;
            }
        }
    }

    public void Cleanup()
    {
        Plugin.Log.LogInfo("Cleaning up Collider Viewer.");
        Instance.Disable();
        foreach (var overlay in Instance._characterOverlays)
        {
            overlay.ParentGameObject.Destroy();
        }
        Instance._playableCharacters.Clear();
        Instance._characterOverlays.Clear();
        Instance._subCharacters.Clear();
        Instance._behaviour?.gameObject.Destroy();
        Instance._behaviour = null;
    }

    public bool IsEnabled()
    {
        if (Instance._behaviour == null) return false;
        return Instance._behaviour.enabled;
    }
}

public static class CollisionColors {
    public static readonly Dictionary<HitBoxType, Color> Colors = new()
    {
        {HitBoxType.Damage, new Color(0, 1, 0, 0.75f)},
        {HitBoxType.Attack, new Color(1, 0, 0, 0.75f)}
    };
}

public class RenderBoxData
{
    public Vector2 RootPosition;
    public Vector2 Offset;
    public Vector2 Size;
    public Box2DCollider? Collider;
    public ObjectDirection DirectionFacing = ObjectDirection.None;
    public HitBoxType Type = HitBoxType.Attack;
}

[HarmonyPatch(typeof(SceneStartupBase), nameof(SceneStartupBase.OnDestroy))]
public static class StopOverlayOnSceneChange
{
    private static void Prefix()
    {
        if (ColliderViewer.Instance.IsEnabled())
        {
            Plugin.Log.LogInfo("Destroying Scene: Disabling Collider Viewer.");
            ColliderViewer.Instance.Cleanup();

        }
    }
}
