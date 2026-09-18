using Enemy;
using Players;
using Unity.Entities.UniversalDelegates;
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
        private const float DefaultDuration = 4.0f;
        private const float DefaultMoveSpeed = 3.0f;
        private const float IceAnimationSlowScale = 0.5f;
        private const float MaxIceAnimationSlow = 0.5f;

        protected EnemyController _ctx;
        protected PlayerController _player;
        protected Rigidbody _rigidbody;

        protected float _moveTimer;
        protected float _duration;

        private float _moveSpeed;
        private float _originMoveSpeed;
        private float _originalAnimSpeed;
        

        public virtual void Init(EnemyController ctx, BaseModuleData data = null)
        {
            _ctx = ctx;
            _rigidbody = ctx.rigidBody;

            _originMoveSpeed = _ctx.stat.MoveSpeed;

            if (data is MoveModuleData mData)
            {
                _duration = mData.duration;
            }
            else
            {
                _duration = DefaultDuration;
            }

            _moveSpeed = _originMoveSpeed;
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
            _rigidbody.linearVelocity = transform.forward * _moveSpeed;
        }

        private void ChangeMoveSpeed(DamageInfo damageInfo, bool isStart)
        {
            // damageInfo.effectDataMap은 모든 EffectType 항목을 항상 갖고 있으므로(폴백 포함),
            // type 플래그를 확인하지 않으면 Fire/Poison/Lightning 등 다른 효과가 시작·종료될 때마다
            // 실제로는 Ice가 아닌데도 여기서 감속이 잘못 걸리거나 풀렸다.
            if (!Utils.HasEffectType(damageInfo.type, EffectType.Ice))
                return;

            if (!damageInfo.effectDataMap.TryGetValue(EffectType.Ice, out var effectData))
            {
                return;
            }
        
            if (isStart)
            {
                float moveSlow = Mathf.Clamp01(effectData.value);
                float animationSlow = Mathf.Min(moveSlow * IceAnimationSlowScale, MaxIceAnimationSlow);

                _moveSpeed = _originMoveSpeed * (1f - moveSlow);
                _ctx.anim.speed = _originalAnimSpeed * (1f - animationSlow);
            }
            else
            {
                _moveSpeed = _originMoveSpeed;
                _ctx.anim.speed = _originalAnimSpeed;
            }
        }
    }
    

    [System.Serializable]
    public class EnemyAttack : MonoBehaviour, IEnemyBehavior, IAnimationListener
    {
        protected EnemyController _ctx;
        protected EnemyStat _stat;
        protected PlayerController _player;

        protected int _animIndex;

        public virtual void Init(EnemyController ctx, BaseModuleData data = null)
        {
            _ctx = ctx;
            _stat = _ctx.stat;

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
            _ctx.rigidBody.linearVelocity = Vector3.zero;
        }

        public virtual void Tick() { }

        public virtual void OnExit()
        {
            _ctx.SetAttackEndTrigger(false);
        }

        public virtual void OnAnimEvent() { }
    }
}