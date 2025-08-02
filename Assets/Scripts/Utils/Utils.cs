using UnityEngine;

public static class Utils
{
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

    public static bool CompareTag(Collider _object, string _tag)
    {
        return _object.transform.root.CompareTag(_tag);
    }

    public static bool CompareTag(Collision collision, string _tag) 
    {
        return collision.transform.root.CompareTag(_tag);
    }
}
