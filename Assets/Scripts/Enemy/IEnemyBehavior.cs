using Enemy;
using Players;
using UnityEngine;

namespace Enemies
{
    public interface IEnemyBehavior
    {
        public void Init(EnemyController ctx, BaseModuleData data = null); 
        public void Tick(); 
        public void OnEnter(); 
        public void OnExit();
    }

    public interface IAnimationListener 
    {
        public void OnAnimEvent();    
    }

    [System.Serializable]
    public class EnemyMove : MonoBehaviour, IEnemyBehavior
    {
        [Header("Default Tuning")]
        [SerializeField] protected float defaultMoveTime = 4f;
        [SerializeField] protected float defaultMoveSpeed = 1f;
    
        protected EnemyController _ctx;
        protected EnemyStats _stats;
        protected PlayerController _player;
        protected float _moveTimer;

        public virtual void Init(EnemyController ctx, BaseModuleData data = null) 
        {
            _ctx = ctx;

            if (!_ctx.isDebugMode) 
            {
                _stats = _ctx.stats;
                defaultMoveSpeed = _stats.baseStats.moveSpeed;
            }

            _moveTimer = defaultMoveTime;
        }

        public virtual void OnEnter() 
        {
            _player = _ctx.player;
        }

        public virtual void OnExit() 
        {
            _ctx.lastPlayerPosition = _player.transform.position;
        }

        public virtual void Tick() 
        {
            UpdateState(EnemyState.Idle);
        }

        protected virtual void UpdateState(EnemyState state) 
        {
            _moveTimer -= Time.deltaTime;

            if (_moveTimer < 0)
            {
                _moveTimer = defaultMoveTime;
                _ctx.ChangeState(state);
                return;
            }
        }

        protected void ForwardMove() 
        {
            transform.position += transform.forward * defaultMoveSpeed * Time.deltaTime;
        }
    }

    [System.Serializable]
    public class EnemyAttack : MonoBehaviour, IEnemyBehavior, IAnimationListener
    {
        protected EnemyController _ctx;
        protected EnemyStats _stats;

        protected PlayerController _player;

        public virtual void Init(EnemyController ctx, BaseModuleData data = null) 
        {
            _ctx = ctx;

            if (!_ctx.isDebugMode)
            {
                _stats = _ctx.stats;
            }
        
            _ctx.SetAttackTrigger(false);
        } 

        public virtual void OnEnter() 
        {
            _player = _ctx.player;

            Vector3 dir = Utils.GetXZDirectionVector(_ctx.lastPlayerPosition, transform.position);
            transform.rotation = Quaternion.LookRotation(dir);

            _ctx.SetAttackTrigger(false);
        }

        public virtual void Tick() 
        {
        
        }

        public virtual void OnExit() 
        {
            _ctx.SetAttackTrigger(false);
        }

        public virtual void OnAnimEvent() { }
    }
}