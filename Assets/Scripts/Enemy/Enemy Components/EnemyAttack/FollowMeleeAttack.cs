using System;
using Enemy;
using UnityEngine;

namespace Enemy
{
    public class FollowMeleeAttack : MeleeAttack
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

            _moveSpeed = ctx.stat.MoveSpeed;

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

            // StateIndex가 지난 공격 사이클의 값(0이 아님)으로 남아있으면 Animator가 Follow를 건너뛰고 바로 공격 스윙으로 진입한다.
            _ctx.anim.SetInteger(AnimHashes.StateIndex, 0);
            
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
            else
            {
                _ctx.rigidBody.linearVelocity = Vector3.zero;
            }
        }

        private void ChasePlayer()
        {
            _chaseTimer -= Time.fixedDeltaTime;

            bool isInRange = Vector3.Distance(_player.transform.position, transform.position) <= _attackRange;
            
            if (_chaseTimer <= 0f || isInRange)
            {
                _isChasing = false;

                _ctx.rigidBody.linearVelocity = Vector3.zero;
                _ctx.rigidBody.angularVelocity = Vector3.zero;

                _ctx.anim.SetInteger(AnimHashes.StateIndex, _animIndex);
                return;
            }
            
            Vector3 dir = Utils.GetDirectionVector(_player.transform, transform);
            transform.rotation = Quaternion.LookRotation(dir);
            
            ChaseMoveForward();
        }

        private void ChaseMoveForward()
        {
            // 이전 상태에서 잠들어 있었을 수 있으므로 안전하게 깨운다.
            _ctx.rigidBody.WakeUp();

            // rigidBody.transform.forward는 리깅 구조에 따라 실제 바라보는 방향과 어긋날 수 있어 transform.forward를 사용한다.
            _ctx.rigidBody.linearVelocity = transform.forward * _moveSpeed;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_ctx.transform.position, _attackRange);
        }
    }
}

