using Game.Enemies;
using Game.Enemies.Enum;
using UnityEngine;

public class BossEnemyController : EnemyController
{
    #region Move Module
    [SerializeField] private RandomMove randomMove;
    [SerializeField] private FollowMove followMove;
    #endregion

    int currentAttackIndex = 0;
    [HideInInspector] public bool isTargeting = false;

    #region Unity Cycle
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Update()
    {
        base.Update();
    }
    #endregion

    #region Initialization

    protected override void ApplyEnemyData(MapData _mapData, int _stageIndex)
    {
        base.ApplyEnemyData(_mapData, _stageIndex);
    }

    protected override void InitMoveModule()
    {
        foreach (var (tag, type) in EnemyBehaviorFactories.MoveFactory)
        {
            // EnemyController가 가지고 있는 EnemyTag와 팩토리 태그가 일치하는지 확인
            if (EnemyTagUtil.Has(enemyTags, tag))  // enemyTags는 EnemyController 필드 가정
            {
                if (tag == EnemyTag.RandomMove) 
                {
                    randomMove = (RandomMove)gameObject.GetOrAddComponent(type); // ← Type 오버로드 사용
                    randomMove.Init(this);
                }
                if (tag == EnemyTag.FollowMove) 
                {
                    followMove = (FollowMove)gameObject.GetOrAddComponent(type);
                    followMove.Init(this);
                }
            }
        }

        // 매칭되는 태그가 없으면 기본 Move 설정 (예: IdleMove)
        if (randomMove == null)
        {
            Debug.LogError("No Enemy Move Tag");
        }
    }

    protected override void InitModule()
    {
        ClearAction();
        InitMoveModule();
        InitAttackModule();

        idle = gameObject.GetOrAddComponent<EnemyIdle>();
        die = gameObject.GetOrAddComponent<EnemyDie>();
        hurt = gameObject.GetOrAddComponent<EnemyHurt>();
        move = (EnemyMove)randomMove;

        actionTable = new() {
      { EnemyState.Idle,   (enter: idle.OnEnter,   exit: idle.OnExit,      tick: idle.Tick) },
      { EnemyState.Move,   (enter: move.OnEnter,   exit: move.OnExit,      tick: move.Tick)  },
      { EnemyState.Attack, (enter: attackOnEnter,  exit: attackOnExit,     tick: attackOnTick)},
      { EnemyState.Hurt,   (enter: null,           exit : null,            tick: null)},
      { EnemyState.Dead,   (enter: die.OnEnter,    exit: die.OnExit,       tick: die.Tick)},
        };
    }
    #endregion

    public override void GetHit(int _damage)
    {
        if (currentState == EnemyState.Dead)
            return;

        health.TakeDamage(_damage);

        if (health.IsDead())
        {
            EnemyManager.enemies.Remove(this);
            ColliderActive(false); // Disable collider on death
            RigidbodyActive(false); // Disable rigidbody on death            
            ChangeState(EnemyState.Dead);
        }
        else
        {
            // 효과만
        }
    }

    public override void ChangeState(EnemyState next)
    {
        if (currentState == next) return;

        if (currentState == EnemyState.Dead) return;

        if (next == EnemyState.Hurt) return;

        if (next == EnemyState.Attack )
        {
            if (!isTargeting)
            {
                currentAttackIndex = Random.Range(0, attackList.Count);
                anim.SetFloat(AttackIndex, currentAttackIndex);

                if (currentAttackIndex == 1 && followMove != null)
                {
                    anim.SetFloat(MoveIndex, 1);

                    onExit?.Invoke();

                    move = followMove;
                    currentState = EnemyState.Move;
                    (onEnter, onExit, onTick) = actionTable[currentState];
                    
                    onEnter?.Invoke();
                    return;
                }
            }
            else 
            {
                isTargeting = false;
                anim.SetFloat(MoveIndex, 0);
                move = randomMove;
            }
        }

        anim.SetBool(animBool[currentState], false);
        anim.SetBool(animBool[next], true);

        onExit?.Invoke();
        currentState = next;
        (onEnter, onExit, onTick) = actionTable[next];
        onEnter?.Invoke();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
