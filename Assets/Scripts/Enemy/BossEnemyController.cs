using Enemies;
using Managers;
using UnityEngine;

namespace Enemy
{
    public class BossEnemyController : EnemyController
    {
        [Header("Boss Move")]
        #region Move Module
        [SerializeField] private RandomMove randomMove;
        [SerializeField] private FollowMove followMove;
        #endregion

        int currentAttackIndex = 0;
        bool isTargeting = false;

        #region Unity Cycle
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
        protected override void InitMoveModule()
        {
            foreach (var (enemyTag, type) in EnemyBehaviorFactories.MoveFactory)
            {
                if (EnemyTagUtil.Has(enemyTags, enemyTag))
                {
                    if (enemyTag == EnemyTag.RandomMove) 
                    {
                        randomMove = (RandomMove)gameObject.GetOrAddComponent(type);
                        randomMove.Init(this);
                    }
                    if (enemyTag == EnemyTag.FollowMove) 
                    {
                        followMove = (FollowMove)gameObject.GetOrAddComponent(type);
                        followMove.Init(this);
                    }
                }
            }

            if (!randomMove)
            {
                Debug.LogError("No Enemy Move Tag");
            }
        }

        protected override void InitModule()
        {
            ClearAction();
            InitMoveModule();
            InitAttackModule();

            idle = gameObject.GetOrAddComponent<EnemyIdle>(); idle.Init(this);
            die = gameObject.GetOrAddComponent<EnemyDie>(); die.Init(this);
            hurt = gameObject.GetOrAddComponent<EnemyHurt>(); hurt.Init(this);
            move = (EnemyMove)randomMove;

            ActionTable = new() {
                { EnemyState.Idle,   (enter: idle.OnEnter,   exit: idle.OnExit,      tick: idle.Tick) },
                { EnemyState.Move,   (enter: move.OnEnter,   exit: move.OnExit,      tick: move.Tick)  },
                { EnemyState.Attack, (enter: OnAttackEnter,  exit: OnAttackExit,     tick: OnAttackTick)},
                { EnemyState.Hurt,   (enter: null,           exit : null,            tick: null)},
                { EnemyState.Dead,   (enter: die.OnEnter,    exit: die.OnExit,       tick: die.Tick)},
            };
        }
        #endregion

        public override void TakeDamage(int damage)
        {
            if (CurrentState == EnemyState.Dead)
                return;

            health.TakeDamage(damage);

            if (health.IsDead())
            {
                EnemyManager.Instance.RemoveEnemy(this);
                ColliderActive(false);
                RigidbodyActive(false);
                ChangeState(EnemyState.Dead);
            }
        }

        public override void ChangeState(EnemyState next)
        {
            if (CurrentState == next) return;

            if (CurrentState == EnemyState.Dead) return;

            if (next == EnemyState.Hurt) return;

            if (next == EnemyState.Attack )
            {
                if (!isTargeting)
                {
                    currentAttackIndex = 0;//Random.Range(0, attacks.Count);

                    anim.SetFloat(AnimHashes.AttackIndex, currentAttackIndex);

                    if (currentAttackIndex == 1 && followMove != null)
                    {
                        isTargeting = true;

                        anim.SetFloat(AnimHashes.MoveIndex, 1);

                        OnExit?.Invoke();

                        move = followMove;
                        ActionTable[EnemyState.Move] = (move.OnEnter, move.OnExit, move.Tick);
                        (OnEnter, OnExit, OnTick) = ActionTable[EnemyState.Move];

                        OnEnter?.Invoke();
                        return;
                    }
                }
                else 
                {
                    isTargeting = false;
                
                    anim.SetFloat(AnimHashes.MoveIndex, 0);

                    move = randomMove;
                    ActionTable[EnemyState.Move] = (move.OnEnter, move.OnExit, move.Tick);
                }
            }

            anim.SetBool(StateAnimHashes[CurrentState], false);
            anim.SetBool(StateAnimHashes[next], true);

            OnExit?.Invoke();
            CurrentState = next;
            (OnEnter, OnExit, OnTick) = ActionTable[next];
            OnEnter?.Invoke();
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
}
