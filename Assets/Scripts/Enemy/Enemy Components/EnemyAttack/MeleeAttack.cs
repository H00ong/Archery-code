using Enemy;
using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    private float _attackMoveSpeed = 3f;
    
    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);

        if (data is MeleeAttackData meleeData)
        {
            _attackMoveSpeed = meleeData.attackMoveSpeed;
        }
    }

    // FollowMeleeAttack은 추격이 끝난 뒤 스윙하므로 진입 시점에 소리를 내지 않는다.
    protected virtual bool PlaySwingSoundOnEnter => true;

    public override void OnEnter()
    {
        base.OnEnter();

        _ctx.SetAttackMoveTrigger(false);

        if (PlaySwingSoundOnEnter)
            PlaySwingSound();
    }

    protected static void PlaySwingSound() => SoundManager.Play(SfxType.EnemyMelee);
    
    public override void Tick()
    {
        base.Tick();

        if (_ctx.AttackMoveTrigger) 
            MoveForward();
        else
            _ctx.rigidBody.linearVelocity = Vector3.zero;
    }

    protected void MoveForward() 
    {
        // rigidBody.transform.forward는 리깅 구조에 따라 실제 바라보는 방향과 어긋날 수 있어 transform.forward를 사용한다.
        _ctx.rigidBody.linearVelocity = transform.forward * _attackMoveSpeed;
    }
}
