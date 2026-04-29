#if UNITY_EDITOR
using System.Collections.Generic;
using Players;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquipmentIdentity))]
public class EquipmentIdentityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var typeProp = serializedObject.FindProperty("equipmentType");
        var nameProp = serializedObject.FindProperty("equipmentName");

        // equipmentType 먼저 그리기
        var iterator = serializedObject.GetIterator();
        iterator.NextVisible(true); // m_Script

        while (iterator.NextVisible(false))
        {
            if (iterator.name == "equipmentName")
            {
                // equipmentType이 이미 그려진 후에 name 팝업 그리기
                DrawEquipmentNamePopup(typeProp, nameProp);
                continue;
            }

            EditorGUILayout.PropertyField(iterator, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEquipmentNamePopup(SerializedProperty typeProp, SerializedProperty nameProp)
    {
        var registry = NameRegistry.Instance;
        if (registry == null)
        {
            EditorGUILayout.PropertyField(nameProp);
            EditorGUILayout.HelpBox("NameRegistry not found.", MessageType.Warning);
            return;
        }

        var equipType = (EquipmentType)typeProp.enumValueIndex;

        List<string> list = equipType switch
        {
            EquipmentType.Weapon => registry.weaponNames,
            EquipmentType.Armor => registry.armorNames,
            EquipmentType.Shoes => registry.shoesNames,
            _ => null
        };

        if (list == null || list.Count == 0)
        {
            EditorGUILayout.PropertyField(nameProp);
            return;
        }

        string[] options = list.ToArray();
        int currentIndex = System.Array.IndexOf(options, nameProp.stringValue);
        if (currentIndex < 0) currentIndex = 0;

        int selectedIndex = EditorGUILayout.Popup("Equipment Name", currentIndex, options);
        nameProp.stringValue = options[selectedIndex];
    }
}
#endif
