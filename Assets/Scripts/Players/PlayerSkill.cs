using Game.Player;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] PlayerManager player;

    public static Dictionary<PlayerSkillId, PlayerSkillModuleBase> acquiredSkillModule = new();
    [SerializeField] private List<SkillDefinition> _skills = new();
    public static List<SkillDefinition> availableSkills = new();

    [Header("For Debug")]
    [SerializeField] SkillDefinition horizontalShotSkill;
    [SerializeField] SkillDefinition dignalShotSkill;
    [SerializeField] SkillDefinition multiShotSkill;


    private void Start()
    {
        Init();
    }

    private void Init()
    {
        availableSkills = _skills;

        foreach (var skill in availableSkills)
        {
            acquiredSkillModule[skill.id] = null;
        }
    }


    public void AcquireSkill(SkillDefinition so)
    {
        if (acquiredSkillModule[so.id] != null)
            UpgradeSkillModule(so);
        else
            LearnSkill(so);
    }

    private void LearnSkill(SkillDefinition so)
    {
        var skill = _skills.Find(skill => skill.id == so.id);
        skill.InstallModule(gameObject);
    }

    private static void UpgradeSkillModule(SkillDefinition so)
    {
        acquiredSkillModule[so.id].LevelUp();

        if (acquiredSkillModule[so.id].level == so.maxLevel)
            acquiredSkillModule.Remove(so.id);
    }

    public static void UpgradeAttackSpeed(float modifier)
    {
        PlayerAttack.playerAttackSpeed *= modifier;
        //player.anim.SetFloat()
    }
}
