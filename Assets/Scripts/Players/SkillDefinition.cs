using UnityEngine;
using Game.Player;
using System;
using UnityEditor;

[CreateAssetMenu(fileName = "PlayerSkillScriptable", menuName = "Scriptable Objects/PlayerSkillScriptable")]
public class SkillDefinition : ScriptableObject
{
    public PlayerSkillId id;
    public SkillRarity rarity;
    public int maxLevel;
    public MonoScript skillModule;

    public void InstallModule(GameObject owner)
    {
        var t = skillModule.GetClass();
        if (t == null || !typeof(MonoBehaviour).IsAssignableFrom(t))
        {
            Debug.LogError($"{id.ToString()}: script type is invalid!");
            return;
        }

        // owner에 모듈 붙이기
        var comp = owner.GetComponent(t) ?? owner.AddComponent(t);
        comp.Init();
    }
}
