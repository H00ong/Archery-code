using System;
using System.Collections.Generic;
using System.ComponentModel;
using Enemies;
using Enemies.Enemy_Components.EnemyAttack;
using Enemy;
using Enemy.Enemy_Components.EnemyAttack;
using UnityEngine;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;

public static class EnemyBehaviorFactory
{
    public static readonly List<(EnemyTag tag, System.Type type)> MoveFactory
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
        // 새로운 공격이 생기면 여기에 한 줄만 추가하면 끝!
    };
    
    private static void ProcessAttackModule(
        EnemyController c,
        EnemyTag tag,
        System.Type moduleType,
        List<EnemyAttack> outList,
        Dictionary<EnemyTag, EnemyAttack> outDict,
        Dictionary<EnemyTag, BaseModuleData> loadedData)
    {
        // 1. 컴포넌트 추가/가져오기 (Type 버전 사용)
        Component comp = c.gameObject.GetOrAddComponent(moduleType);

        // 2. EnemyAttack으로 형변환 (모든 공격 모듈은 EnemyAttack 상속받으므로 가능)
        if (comp is EnemyAttack module)
        {
            // 3. 데이터 가져오기 (없으면 null)
            var data = loadedData?.GetValueOrDefault(tag);

            // 4. 초기화 (데이터 주입)
            module.Init(c, data);

            // 5. 리스트 및 딕셔너리 등록
            if (!outList.Contains(module)) 
                outList.Add(module);
            
            // 주의: 딕셔너리 키는 현재 'tag'를 그대로 사용합니다.
            // 만약 Tag(MeleeAttack)와 Key(Melee)가 다르다면 매핑 테이블을 수정해야 합니다.
            outDict[tag] = module;
        }
    }

    public static void CreateAttackModules(
        EnemyController c, 
        EnemyTag tags, 
        List<EnemyAttack> outList, 
        Dictionary<EnemyTag, EnemyAttack> outDict,
        Dictionary<EnemyTag, BaseModuleData> loadedData = null // 데이터 딕셔너리 (없으면 null)
    )
    {
        foreach (var kvp in AttackModuleRegistry)
        {
            EnemyTag targetTag = kvp.Key;   // 예: EnemyTag.FlyingShoot
            System.Type type = kvp.Value;   // 예: typeof(FlyingShoot)

            // 적이 해당 태그를 가지고 있다면?
            if (EnemyTagUtil.Has(tags, targetTag))
            {
                // 공통 로직 수행
                ProcessAttackModule(c, targetTag, type, outList, outDict, loadedData);
            }
        }
    }
}