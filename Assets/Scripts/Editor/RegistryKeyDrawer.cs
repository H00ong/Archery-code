#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RegistryKeyAttribute))]
public class RegistryKeyDrawer : PropertyDrawer
{
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var attr = (RegistryKeyAttribute)attribute;
        var regisry = NameRegistry.Instance;

        if (regisry == null)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.HelpBox(
                new Rect(position.x, position.yMax, position.width, 20),
                "NameRegistry not found in Resources/", MessageType.Warning);
            return;
        }

        List<string> list = attr.ListName switch
        {
            "enemyNames" => regisry.enemyNames,
            "mapIds" => regisry.mapIds,
            "characterNames" => regisry.characterNames,
            "skillIds" => regisry.skillIds,
            _ => null
        };

        if (list == null || list.Count == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        string[] options = list.ToArray();
        int currentIndex = System.Array.IndexOf(options, property.stringValue);
        if (currentIndex < 0)
            currentIndex = 0;

        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
        property.stringValue = options[selectedIndex];
    }
}
#endif