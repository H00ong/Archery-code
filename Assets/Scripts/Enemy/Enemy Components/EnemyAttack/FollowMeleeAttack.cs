using System;
using Enemy;
using UnityEngine;

namespace Enemy
{
    public class FollowMeleeAttack : MeleeAttack, IAnimationListener
    {
        private const float DefaultMoveSpeed = 3f;
        private const float DefaultAttackRange = 2f;
        private const float DefaultChaseDuration = 3f;
        private const float ChaseMoveSpeedMultiplier = 1.5f;

        private float _attackRange;
        private float _chaseDuration;
        private float _chaseTimer;
        private float _moveSpeed;

        private bool _isChasing;
        
        public override void Init(EnemyController ctx, BaseModuleData data = null)
        {
            base.Init(ctx, data);

            if (!_ctx.isDebugMode)
            {
                _moveSpeed = ctx.stat.MoveSpeed;
            }
            else
            {
                _moveSpeed = DefaultMoveSpeed;
            }

            if (data is FollowMeleeAttackData fmData)
            {
                _attackRange = fmData.attackRange;
                _chaseDuration = fmData.chaseDuration;
                _moveSpeed *= fmData.moveSpeedIncreaseMultiplier;
            }
            else
            {
                _attackRange = DefaultAttackRange;
                _chaseDuration = DefaultChaseDuration;
                _moveSpeed *= ChaseMoveSpeedMultiplier;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            
            _chaseTimer = _chaseDuration;
            _isChasing = true;
            
            if(_ctx.HasMultiAttackModules)
            {
                _ctx.anim.SetInteger(AnimHashes.AttackIndex, _animIndex);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Tick()
        {
            base.Tick();
            
            if (_isChasing)
            {
                ChasePlayer();
                return;
            }

            if (_ctx.AttackMoveTrigger)
            {
                MoveForward();
            }
        }

        private void ChasePlayer()
        {
            _chaseTimer -= Time.fixedDeltaTime;

            bool isInRange = Vector3.Distance(_player.transform.position, transform.position) <= _attackRange;
            
            if (_chaseTimer <= 0f || isInRange)
            {
                _isChasing = false;
                _ctx.anim.SetInteger(AnimHashes.StateIndex, _animIndex);
                return;
            }
            
            Vector3 dir = Utils.GetDirectionVector(_player.transform, transform);
            transform.rotation = Quaternion.LookRotation(dir);
            
            ChaseMoveForward();
        }

        private void ChaseMoveForward()
        {
            Vector3 targetPos = transform.position + transform.forward * _moveSpeed * Time.fixedDeltaTime;
            _ctx.rigidBody.MovePosition(targetPos);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_ctx.transform.position, _attackRange);
        }
    }
}

