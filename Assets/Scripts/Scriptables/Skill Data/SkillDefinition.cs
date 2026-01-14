using UnityEngine;
using Game.Player;
using System;
using Players;
using Players.SkillModule;
using UnityEditor;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "PlayerSkillScriptable", menuName = "Scriptable Objects/PlayerSkillScriptable")]
public class SkillDefinition : ScriptableObject
{
    public PlayerSkillId id;
    public SkillRarity rarity;
    public int maxLevel;
    public MonoScript moduleScript;
    private PlayerSkillModuleBase skillModule;

    public void InstallModule(GameObject owner, PlayerSkill skill)
    {
        var t = moduleScript.GetClass();
        if (t == null || !typeof(MonoBehaviour).IsAssignableFrom(t))
        {
            Debug.LogError($"{id.ToString()}: script type is invalid!");
            return;
        }

        var comp = owner.GetComponent(t) ?? owner.AddComponent(t);

        if (comp is PlayerSkillModuleBase mod)
        {
            mod.Init(skill);
            skillModule = mod;
        }
    }

    public PlayerSkillModuleBase GetModule() => skillModule;
}
