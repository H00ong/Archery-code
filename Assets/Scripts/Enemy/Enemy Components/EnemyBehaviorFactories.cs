using System;
using System.Collections.Generic;
using System.ComponentModel;
using Enemies;
using Enemies.Enemy_Components.EnemyAttack;
using Enemy;
using UnityEngine;
using UnityEngine.InputSystem;

public static class EnemyBehaviorFactories
{
    public static readonly List<(EnemyTag tag, System.Type type)> MoveFactory
    = new()
    {
        (EnemyTag.FollowMove,  typeof(FollowMove)),
        (EnemyTag.PatternMove, typeof(PatternMove)),
        (EnemyTag.RandomMove,  typeof(RandomMove))
    };

    public static void CreateAttackModules(EnemyController c, EnemyTag tags, List<EnemyAttack> outList, Dictionary<EnemyTag, EnemyAttack> outDict)
    {
        if (EnemyTagUtil.Has(tags, EnemyTag.MeleeAttack)) 
        {
            var module = c.gameObject.GetOrAddComponent<MeleeAttack>();
            if(!outList.Contains(module))
                outList.Add(module);

            outDict[EnemyTag.Melee] = module;

        }

        if (EnemyTagUtil.Has(tags, EnemyTag.Shoot))
        {
            var module = c.gameObject.GetOrAddComponent<Shoot>();
            if (!outList.Contains(module))
                outList.Add(module);
            outDict[EnemyTag.Shoot] = module;
        }

        if (EnemyTagUtil.Has(tags, EnemyTag.FlyingShoot)) 
        {
            var module = c.gameObject.GetOrAddComponent<FlyingShoot>();
            if (!outList.Contains(module))
                outList.Add(module);
            outDict[EnemyTag.FlyingShoot] = module;
        }
    }
}