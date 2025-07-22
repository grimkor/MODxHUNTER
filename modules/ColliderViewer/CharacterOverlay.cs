using System;
using System.Collections.Generic;
using System.Linq;
using ECC;
using Henley.Battle;
using UnityEngine;
using Object = UnityEngine.Object;

namespace modxhunter.modules.ColliderViewer;

public class CharacterOverlay
{
    private readonly PlayableCharacterCore _owner;
    public readonly GameObject ParentGameObject = new()
    {
        hideFlags = HideFlags.HideAndDontSave,
        active = false,
        name = "mxh-overlay-UNITIALISED"
    };

    private readonly List<(Transform, SpriteRenderer)> _attackRenderers = new();
    private readonly List<(Transform, SpriteRenderer)> _damageRenderers = new();
    private protected readonly Texture2D TEX = new(512, 512, TextureFormat.ARGB32, false);
    private protected readonly Texture2D TEXAttack = new(512, 512, TextureFormat.ARGB32, false);
    private protected readonly Texture2D TEXDamage = new(512, 512, TextureFormat.ARGB32, false);
    protected readonly Sprite SpriteAttack = new();
    protected readonly Sprite SpriteDamage = new();

    public CharacterOverlay(PlayableCharacterCore player)
    {
        _owner = player;
        TEXAttack.SetPixels(Enumerable.Repeat(Color.white, 512 * 512).ToArray());
        TEXAttack.Apply();
        TEXDamage.SetPixels(Enumerable.Repeat(Color.white, 512 * 512).ToArray());
        TEXDamage.Apply();
        SpriteAttack = Sprite.Create(
            TEXAttack,
            new(1, 1, 256, 256),
            Vector2.one,
            128,
            UInt32.MinValue,
            SpriteMeshType.FullRect
        );
        SpriteDamage = Sprite.Create(
            TEXDamage,
            new(1, 1, 256, 256),
            Vector2.one,
            128,
            UInt32.MinValue,
            SpriteMeshType.FullRect
        );
        InitParent(player.CharacterId);
        GenerateDamageBoxObjects(
            player.CharacterId,
            player.Collision.GetHitBox(HitBoxType.Damage, player._actionName, 0).ColliderCount
        );
        GenerateAttackBoxObjects(player.CharacterId, 20);
    }

    protected CharacterOverlay()
    {
    }

    protected void InitParent(string id)
    {
        TEX.Apply();
        ParentGameObject.name = $"mxh-overlay-{id}";
        ParentGameObject.hideFlags = HideFlags.HideAndDontSave;
        Object.DontDestroyOnLoad(ParentGameObject);
    }

    private protected void GenerateDamageBoxObjects(string id, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var overlayObj = new GameObject();
            overlayObj.name = $"mxh-overlay{id}-dmg-{i}";
            overlayObj.SetParent(ParentGameObject);
            var rectTransform = overlayObj.AddComponent<RectTransform>();
            var sprite = overlayObj.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteDamage;
            sprite.size = Vector2.one;
            sprite.drawMode = SpriteDrawMode.Tiled;
            sprite.color = CollisionColors.Colors[HitBoxType.Damage];
            overlayObj.transform.localScale = Vector3.one;
            _damageRenderers.Add((rectTransform, sprite));
        }
    }

    private protected void GenerateAttackBoxObjects(string id, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var overlayObj = new GameObject();
            overlayObj.name = $"mxh-overlay{id}-atk-{i}";
            overlayObj.SetParent(ParentGameObject);
            var rectTransform = overlayObj.AddComponent<RectTransform>();
            var sprite = overlayObj.AddComponent<SpriteRenderer>();
            sprite.sprite = SpriteAttack;
            sprite.size = Vector2.one;
            sprite.drawMode = SpriteDrawMode.Tiled;
            sprite.color = CollisionColors.Colors[HitBoxType.Attack];
            overlayObj.transform.localScale = Vector3.one;
            _attackRenderers.Add((rectTransform, sprite));
        }
    }

    public List<(Transform, SpriteRenderer)> GetRenderTargets(HitBoxType type)
    {
        switch (type)
        {
            case HitBoxType.Attack:
                return _attackRenderers;
            case HitBoxType.Damage:
                return _damageRenderers;
            case HitBoxType.Throw:
            case HitBoxType.Guard:
            default:
                return new();
        }
    }

    virtual public PlayableCharacterCore GetCharacter()
    {
        return _owner;
    }

    public virtual Vector2 GetCharacterPosition()
    {
        throw new NotImplementedException();
    }
}

public class SubCharacterOverlay : CharacterOverlay
{
    private SubCharacterCore? _owner;

    public SubCharacterOverlay(SubCharacterCore? owner)
    {
        _owner = owner;
        if (_owner == null) return;
        InitParent(_owner.SubCharaId);
        GenerateAttackBoxObjects(_owner.CharacterId, 20);
    }

    public bool ShouldRender()
    {
        var result = _owner != null && _owner.IsActive();
        return result;
    }

    public List<Box2DCollider> GetAttackColliders()
    {
        if (_owner == null) return new();
        var list = _owner.Collision.GetHitBox(HitBoxType.Attack, _owner.CurrentActionName,
            _owner.ActionSystem.RealTimeFrame);
        if (list == null) return new();
        return list._colliderArray.ToList();
    }

    public ObjectDirection GetCharacterDirection()
    {
        if (_owner == null) return ObjectDirection.None;
        return _owner.Direction;
    }

    public override Vector2 GetCharacterPosition() {
        if (_owner == null) return new(0, 0);
        return new(_owner.PositionX, _owner.PositionY);
    }

}
