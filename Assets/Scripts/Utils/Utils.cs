using System;
using System.Collections.Generic;
using UnityEngine;

public enum TagType
{
    Player,
    Enemy,
    Obstacle,
    PlayerProjectile,
    EnemyProjectile,
    Floor
}

public static class Utils
{
    private static readonly Dictionary<TagType, string> Map = new()
    {
        { TagType.Player,           "Player" },
        { TagType.Enemy,            "Enemy" },
        { TagType.Obstacle,         "Obstacle" },
        { TagType.PlayerProjectile, "PlayerProjectile" },
        { TagType.EnemyProjectile,  "EnemyProjectile" },
        { TagType.Floor,            "Floor" },
    };

    public static string ToString(TagType t) => Map[t];

    public static Component GetOrAddComponent(this GameObject obj, Type type)
    {
        var comp = obj.GetComponent(type);
        if (!comp) comp = obj.AddComponent(type);
        return comp;
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        if (!obj.TryGetComponent<T>(out T comp))
        { 
            return obj.AddComponent<T>();
        }

        return comp;
    }


    public static Vector3 GetXZDirectionVector(Vector3 dest, Vector3 source) 
    {
        Vector3 dir = dest - source;
        dir.y = 0;

        return dir.normalized;
    }

    public static Vector3 GetDirectionVector(Transform target, Transform source)
        => GetXZDirectionVector(target.position, source.position);

    public static float GetXZDistance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
    }

    public static GameObject GetCollisionRoot(Collider other) 
    {
        return other.attachedRigidbody != null ? other.attachedRigidbody.gameObject
                                               : other.gameObject;
    }

    public static Vector3 GetXZPosition(Vector3 pos) 
    {
        return new Vector3(pos.x, 0, pos.z);
    }

    public static bool HasEffectType(EffectType mask, EffectType t) => (mask & t) != 0;
    
    public static bool HasFlag(Enemy.EnemyTag mask, Enemy.EnemyTag t) => (mask & t) != 0;

    public static EffectType BarrelTypeToEffectType(Players.BarrelType type)
    {
        return type switch
        {
            Players.BarrelType.Venom => EffectType.Poison,
            Players.BarrelType.Blaze => EffectType.Fire,
            Players.BarrelType.Ice => EffectType.Ice,
            Players.BarrelType.Lightning => EffectType.Lightning,
            _ => EffectType.Normal
        };
    }

    public static EffectType OrbTypeToEffectType(Players.OrbType type)
    {
        return type switch
        {
            Players.OrbType.Venom => EffectType.Poison,
            Players.OrbType.Blaze => EffectType.Fire,
            Players.OrbType.Ice => EffectType.Ice,
            Players.OrbType.Lightning => EffectType.Lightning,
            _ => EffectType.Normal
        };
    }
}
