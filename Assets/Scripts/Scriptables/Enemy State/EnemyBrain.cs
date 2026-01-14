using Enemy;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStateBrain", menuName = "Enemy/Brain")]
public class EnemyBrain : ScriptableObject
{
    [Header("State Transitions")]
    [SerializeField] protected EnemyState stateAfterMove = EnemyState.Attack;
    [SerializeField] protected EnemyState stateAfterAttack = EnemyState.Idle;
    [SerializeField] protected EnemyState stateAfterHurt = EnemyState.Move;
    [SerializeField] protected EnemyState stateAfterIdle = EnemyState.Move;

    // 모듈이 끝났을 때 다음 상태 결정
    public virtual EnemyState GetNextState(EnemyState currentState, EnemyController controller)
    {
        switch (currentState)
        {
            case EnemyState.Move:
                return stateAfterMove;
            case EnemyState.Attack:
                return stateAfterAttack;
            case EnemyState.Hurt:
                return stateAfterHurt;
            case EnemyState.Idle:
                return stateAfterIdle;
            default:
                return EnemyState.Idle;
        }
    }
    
    public virtual bool ShouldReactToHit(EnemyController controller)
    {
        return true; // 일반 몬스터는 true
    }

    // 어떤 공격을 할지 선택 (보스는 여기서 랜덤/순차 패턴 결정)
    public virtual int SelectAttackIndex(EnemyController controller)
    {
        return 0; // 일반 몬스터는 기본 0번 공격
    }
}
