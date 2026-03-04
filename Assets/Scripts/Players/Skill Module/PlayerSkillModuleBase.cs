using System.Collections.Generic;
using Game.Player.Attack;
using UnityEngine;

namespace Players.SkillModule
{
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

    public interface IPlayerUpgrader
    {
        public void Apply();
    }

    public interface IBulletUpgrader
    {
        public void Apply();
    }

    public interface IShootContributor
    {
        public void AddBullet(List<ShotInstruction> bulletList, ShotInstruction inst);
    }

    public interface IOrbGenerator
    {
        public void GenerateOrb();
    }

    public interface IPickupBarrel
    {
        public void GenerateActivate();
    }
}