#if UNITY_EDITOR
using Game.Enemies.Enum;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyTag))]
public class EnemyTagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // EnemyTag는 ulong 기반 → long으로 다루고 EnumFlagsField 사용
        property.longValue = (int)(EnemyTag)EditorGUI.EnumFlagsField(
            position,
            label,
            (EnemyTag)property.longValue
        );
    }
}
#endif
