using Game.Player.Attack;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkillModuleBase : MonoBehaviour
{
    protected PlayerSkill playerSkill;
    public int Level { get; protected set; } = 1;
    
    public virtual void UpdateSkill() 
    {
        Level++;
    }

    public virtual void Init(PlayerSkill _skill) 
    {
        playerSkill = _skill;
    }
}

// ---------------------------
// 플레이어 능력치 강화 계열
// ---------------------------
public interface IPlayerUpgrader
{
    /// <summary>공격력 배수 또는 가산치 반환</summary>
    public void Apply();
}

// ---------------------------
// 플레이어 탄환 업그레이드 계열 - 벽 반사, 적 관통, 반동 화살, 트래킹 아이
// ---------------------------

public interface IBulletUpgrader
{
    public void Apply();
}

// ---------------------------
// 플레이어 탄환 수 증가 계열
// ---------------------------

public interface IShootContributor
{
    public void AddBullet(List<ShotInstruction> _bulletList, ShotInstruction _default);
}

// ---------------------------
// 플레이어 오브 계열
// ---------------------------

public interface IOrbGenerator
{
    // 오브 매니저를 두고 거기에 등록하는 시스템으로 진행
    public void GenerateOrb();
}

// ---------------------------
// 플레이어 공중 폭격 계열
// ---------------------------

public interface IPickupActivatedBombardment
{
    public void GenerateItem();
}

// ---------------------------
// 펫 계열
// ---------------------------

