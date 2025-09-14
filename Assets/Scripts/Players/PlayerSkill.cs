using Game.Player;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] PlayerManager player;
    [SerializeField] PlayerAttack attack;
    [SerializeField] PlayerMovement move;


    public static Dictionary<PlayerSkillId, PlayerSkillModuleBase> acquiredSkillModule = new();

    [SerializeField] private List<SkillDefinition> _skills = new();
    public static List<SkillDefinition> availableSkills = new();

    [Header("For Debug")]
    [SerializeField] SkillDefinition horizontalShotSkill;
    [SerializeField] SkillDefinition diagonalShotSkill;
    [SerializeField] SkillDefinition multiShotSkill;
    [SerializeField] SkillDefinition attackSpeedUpgradeSkill;
    [SerializeField] SkillDefinition moveSpeedUpgradeSKill;
    [SerializeField] SkillDefinition iceOrb;
    [SerializeField] SkillDefinition venomOrb;

    public void Init()
    {
        availableSkills = _skills;
    }

    private void Update()
    {
        Test();
    }   

    private void Test()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AcquireSkill(horizontalShotSkill);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AcquireSkill(diagonalShotSkill);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AcquireSkill(multiShotSkill);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AcquireSkill(attackSpeedUpgradeSkill);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            AcquireSkill(moveSpeedUpgradeSKill);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            AcquireSkill(iceOrb);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            AcquireSkill(venomOrb);
        }
    }

    public void AcquireSkill(SkillDefinition so)
    {
        if (!availableSkills.Contains(so)) return;

        if (acquiredSkillModule.ContainsKey(so.id))
            UpgradeSkillModule(so);
        else
            LearnSkill(so);
    }

    private void LearnSkill(SkillDefinition so)
    {
        var skill = _skills.Find(skill => skill.id == so.id);
        skill.InstallModule(gameObject, this);
        acquiredSkillModule[so.id] = skill.GetModule();
    }

    private void UpgradeSkillModule(SkillDefinition so)
    {
        acquiredSkillModule[so.id].UpdateSkill();

        if (acquiredSkillModule[so.id].Level >= so.maxLevel)
            availableSkills.Remove(so);
    }


    #region Skill methods

    public void UpgradeAttackSpeed(float modifier) => attack.UpdateAttackSpeed(modifier);
    public void UpgradeMoveSpeed(float modifier)   => move.UpdateMoveSpeed(modifier);

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (player == null) GetComponent<PlayerManager>();
        if (attack == null) GetComponent<PlayerAttack>();
        if (move == null)   GetComponent<PlayerMovement>();
    }
#endif
}
