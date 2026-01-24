using Enemy;
using Players;
using UnityEngine;

namespace Enemy
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

    public class EnemyMove : MonoBehaviour, IEnemyBehavior
    {
        protected EnemyController _ctx;
        protected PlayerController _player;
        
        protected float _moveTimer;
        protected float _duration;
        protected float _defaultMoveSpeed = 3.0f;

        public virtual void Init(EnemyController ctx, BaseModuleData data = null)
        {
            _ctx = ctx;

            if (!_ctx.isDebugMode)
            {
                _defaultMoveSpeed = _ctx.stats.baseStats.moveSpeed;
            }

            if (data is MoveModuleData mData)
            {
                _duration = mData.duration;
            }
            else
            {
                _duration = 4.0f; // 기본값
            }
        }

        public virtual void Tick()
        {
            _moveTimer -= Time.deltaTime;

            if (_moveTimer < 0)
            {
                _ctx.OnModuleComplete();
            }
        }

        public virtual void OnEnter()
        {
            _player = _ctx.player;
            _moveTimer = _duration;
        }

        public virtual void OnExit()
        {
        }

        protected void MoveForward()
        {
            transform.position += transform.forward * _defaultMoveSpeed * Time.deltaTime;
        }
    }

    [System.Serializable]
    public class EnemyAttack : MonoBehaviour, IEnemyBehavior, IAnimationListener
    {
        protected EnemyController _ctx;
        protected EnemyStats _stats;

        protected PlayerController _player;
        protected int _animIndex;

        public virtual void Init(EnemyController ctx, BaseModuleData data = null)
        {
            _ctx = ctx;

            if (!_ctx.isDebugMode)
            {
                _stats = _ctx.stats;
            }

            _ctx.SetAttackEndTrigger(false);
            
            if (data is AttackModuleData aData)
            {
                _animIndex = aData.attackIndex;
            }
        }

        public virtual void OnEnter()
        {
            _player = _ctx.player;
            _ctx.lastPlayerPosition = _player.transform.position;

            Vector3 dir = Utils.GetXZDirectionVector(_ctx.lastPlayerPosition, transform.position);
            transform.rotation = Quaternion.LookRotation(dir);

            _ctx.SetAttackEndTrigger(false);
        }

        public virtual void Tick()
        {
            
        }

        public virtual void OnExit()
        {
            _ctx.SetAttackEndTrigger(false);
        }

        public virtual void OnAnimEvent()
        {
        }
    }
}