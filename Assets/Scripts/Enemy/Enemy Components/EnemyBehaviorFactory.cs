using System;
using System.Collections.Generic;
using System.ComponentModel;
using Enemy;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;

public static class EnemyBehaviorFactory
{
    public static readonly List<(EnemyTag tag, System.Type type)> MoveModuleRegistry
    = new()
    {
        (EnemyTag.FollowMove,  typeof(FollowMove)),
        (EnemyTag.PatternMove, typeof(PatternMove)),
        (EnemyTag.RandomMove,  typeof(RandomMove))
    };
    
    private static readonly Dictionary<EnemyTag, System.Type> AttackModuleRegistry = new()
    {
        { EnemyTag.MeleeAttack, typeof(MeleeAttack) },
        { EnemyTag.Shoot,       typeof(Shoot) },
        { EnemyTag.FlyingShoot, typeof(FlyingShoot) },
        { EnemyTag.FollowMeleeAttack, typeof(FollowMeleeAttack) },
        // 새로운 공격이 생기면 여기에 한 줄만 추가하면 끝!
    };
    
    private static void ProcessAttackModule(
        EnemyController c,
        EnemyTag tag,
        System.Type moduleType,
        BaseModuleData loadedData,
        Dictionary<EnemyTag, IEnemyBehavior> outDict, 
        List<EnemyAttack> outList)
    {
        Component comp = c.gameObject.GetOrAddComponent(moduleType);
        
        if (comp is not EnemyAttack atk) return;
        
        atk.Init(c, loadedData);
        outDict[tag] = atk;
        outList.Add(atk);
    }

    public static void CreateAttackModules(
        EnemyController c,
        EnemyName name,
        EnemyTag tags, 
        Dictionary<EnemyTag, IEnemyBehavior> outDict,
        List<EnemyAttack> outList
    )
    { 
        EnemyTag attributeTags = tags & EnemyTag.AttributeMask;
        
        foreach (var kvp in AttackModuleRegistry)
        {
            EnemyTag attackTag = kvp.Key;
            Type type = kvp.Value;   // ex : typeof(FlyingShoot)

            if (EnemyTagUtil.Has(tags, attackTag))
            {
                EnemyTag specificTag = attackTag | attributeTags;
                var key1 = new EnemyKey(name, specificTag);
                
                var foundData = EnemyManager.Instance.GetModuleData(key1);
                
                ProcessAttackModule(c, attackTag, type, foundData, outDict, outList);
            }
        }
    }
    
    public static EnemyMove CreateMoveModules(
        EnemyController c,
        EnemyName enemyName,
        EnemyTag tags,
        Dictionary<EnemyTag, IEnemyBehavior> outModules
    )
    {
        foreach (var (targetTag, type) in MoveModuleRegistry)
        {
            if (EnemyTagUtil.Has(tags, targetTag))
            {
                var key = new EnemyKey(enemyName, targetTag); 
                BaseModuleData loadedData = EnemyManager.Instance.GetModuleData(key);
                
                var comp = c.gameObject.GetOrAddComponent(type);
                
                if (comp is EnemyMove module)
                {
                    module.Init(c, loadedData);
                    outModules.TryAdd(targetTag, module);
                    
                    return module;
                }
            }
        }
        
        return null;
    }
}