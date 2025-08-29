using Game.Enemies.Enum;
using Game.Enemies;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public static class EnemyBehaviorFactories
{
    // Move 동작 생성용 팩토리 리스트
    // 리스트의 각 항목 = (EnemyTag, 생성할 함수)
    public static readonly List<(EnemyTag tag, System.Type type)> MoveFactory
    = new()
    {
        (EnemyTag.FollowMove,  typeof(FollowMove)),
        (EnemyTag.PatternMove, typeof(PatternMove)),
        (EnemyTag.RandomMove,  typeof(RandomMove))
    };

    // 공격 모듈은 여러 개 붙을 수 있으므로 List에 추가하는 방식
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