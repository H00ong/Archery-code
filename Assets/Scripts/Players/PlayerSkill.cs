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
    [SerializeField] SkillDefinition iceBarrel;

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
        if (Input.GetKeyDown(KeyCode.Alpha8)) 
        {
            AcquireSkill(iceBarrel);
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

    public static int GetLevel(SkillDefinition def)
    {
        return acquiredSkillModule.TryGetValue(def.id, out var mod) ? mod.Level : 0;
    }

    public static List<SkillDefinition> GetRandomChoices(int count = 3)
    {
        // 이미 availableSkills는 "배울 수 있는 것만" 포함한다고 하셨으므로 필터 불필요
        var rng = new System.Random();
        // 간단 셔플 후 take
        var pool = new List<SkillDefinition>(availableSkills);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.Count <= count ? pool : pool.GetRange(0, count);
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
