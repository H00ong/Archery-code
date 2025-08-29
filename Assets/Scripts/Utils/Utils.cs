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


    public static Vector3 GetDirectionVector(Vector3 _dest, Vector3 _source) 
    {
        Vector3 dir = _dest - _source;
        dir.y = 0; // Ignore vertical component

        return dir.normalized;
    }

    public static Vector3 GetDirectionVector(Transform _target, Transform _source)
        => GetDirectionVector(_target.position, _source.position);

    public static float GetXZDistance(Vector3 _a, Vector3 _b)
    {
        return Mathf.Sqrt(Mathf.Pow(_a.x - _b.x, 2) + Mathf.Pow(_a.z - _b.z, 2));
    }

    public static bool CompareTag(Transform _object, string _tag) 
    {
        return _object.root.CompareTag(_tag);
    }

    public static bool CompareTag(GameObject _object, string _tag)
    {
        return _object.transform.root.CompareTag(_tag);
    }

    public static bool CompareRootTag(Collider _object, string _tag)
    {
        return _object.transform.root.CompareTag(_tag);
    }

    public static bool CompareTag(Collision collision, string _tag) 
    {
        return collision.transform.root.CompareTag(_tag);
    }
}
