using System;
using UnityEngine;

public static class Utils
{
    public static Component GetOrAddComponent(this GameObject obj, Type type)
    {
        var comp = obj.GetComponent(type);
        if (comp == null) comp = obj.AddComponent(type);
        return comp;
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        if (comp == null)
            comp = obj.AddComponent<T>();
        return comp;
    }


    public static Vector3 GetDirectionVector(Vector3 dest, Vector3 source) 
    {
        Vector3 dir = dest - source;
        dir.y = 0; // Ignore vertical component

        return dir.normalized;
    }

    public static Vector3 GetDirectionVector(Transform target, Transform source)
        => GetDirectionVector(target.position, source.position);

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

}
