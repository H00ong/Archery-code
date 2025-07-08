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
}
