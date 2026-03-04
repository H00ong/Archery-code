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
        private float _defaultMoveSpeed = 3.0f;
        private float _moveSpeed;
        private float _originalAnimSpeed;
        

        public virtual void Init(EnemyController ctx, BaseModuleData data = null)
        {
            _ctx = ctx;

            if (!_ctx.isDebugMode)
            {
                _defaultMoveSpeed = _ctx.stat.MoveSpeed;
            }

            _moveSpeed = _defaultMoveSpeed;

            if (data is MoveModuleData mData)
            {
                _duration = mData.duration;
            }
            else
            {
                _duration = 4.0f; // 기본값
            }

            _originalAnimSpeed = _ctx.anim.speed;

            _ctx.health.OnStatusChanged -= ChangeMoveSpeed;
            _ctx.health.OnStatusChanged += ChangeMoveSpeed;
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
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;
        }

        private void ChangeMoveSpeed(DamageInfo damageInfo, bool isStart)
        {
            if(!damageInfo.effectDataMap.TryGetValue(EffectType.Ice, out var effectData))
            {
                return;
            }
        
            if (isStart)
            {
                _moveSpeed = _defaultMoveSpeed * (1f - effectData.value);
                _ctx.anim.speed = _originalAnimSpeed * (1f - effectData.value);
            }
            else
            {
                _moveSpeed = _defaultMoveSpeed;
                _ctx.anim.speed = _originalAnimSpeed;
            }
        }
    }
    

    [System.Serializable]
    public class EnemyAttack : MonoBehaviour, IEnemyBehavior, IAnimationListener
    {
        protected EnemyController _ctx;
        protected EnemyStat _stats;

        protected PlayerController _player;
        protected int _animIndex;

        public virtual void Init(EnemyController ctx, BaseModuleData data = null)
        {
            _ctx = ctx;

            if (!_ctx.isDebugMode)
            {
                _stats = _ctx.stat;
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