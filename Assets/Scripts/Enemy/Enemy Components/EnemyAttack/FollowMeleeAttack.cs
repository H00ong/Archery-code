using System;
using Enemy;
using UnityEngine;

namespace Enemy
{
    public class FollowMeleeAttack : MeleeAttack, IAnimationListener
    {
        private float _attackRange = 1.5f;
        private float _chaseDuration = 3f;
        private float _chaseTimer;
        private float _moveSpeed = 3f;

        private bool _isChasing;
        
        public override void Init(EnemyController ctx, BaseModuleData data = null)
        {
            base.Init(ctx, data);

            if (!_ctx.isDebugMode)
            {
                _moveSpeed = ctx.stat.MoveSpeed;
            }

            if (data is not FollowMeleeAttackData fmData)
            {
                Debug.LogError("[FollowMeleeAttack] Invalid module data provided!");
                return;
            }

            _attackRange = fmData.attackRange;
            _chaseDuration = fmData.chaseDuration;
            _moveSpeed *= fmData.moveSpeedIncreaseMultiplier;
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
            
            _ctx.anim.SetInteger(AnimHashes.StateIndex, 0);
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
            _chaseTimer -= Time.deltaTime;
            bool isInRange = Vector3.Distance(_player.transform.position, transform.position) <= _attackRange;
            
            if (_chaseTimer <= 0f || isInRange)
            {
                _isChasing = false;
                _ctx.anim.SetInteger(AnimHashes.StateIndex, _animIndex);
                return;
            }
            
            Vector3 dir = Utils.GetDirectionVector(_player.transform, transform);
            transform.rotation = Quaternion.LookRotation(dir);
            
            if (!_ctx.IsBlocked) ChaseMoveForward();
        }

        private void ChaseMoveForward()
        {
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_ctx.transform.position, _attackRange);
        }
    }
}

