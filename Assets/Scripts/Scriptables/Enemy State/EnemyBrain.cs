using UnityEngine;

namespace Enemy
{
    [CreateAssetMenu(fileName = "EnemyBrain", menuName = "Enemy/Brain/EnemyBrain")]
    public class EnemyBrain : ScriptableObject
    {
        [Header("State Flow")]
        [SerializeField] private EnemyState afterIdle;
        [SerializeField] private EnemyState afterMove;
        [SerializeField] private EnemyState afterAttack;
        [SerializeField] private EnemyState afterHurt;

        public EnemyState GetNextAction(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Idle:   return afterIdle;
                case EnemyState.Move:   return afterMove;
                case EnemyState.Attack: return afterAttack; 
                case EnemyState.Hurt:   return afterHurt;
                default:                return afterIdle;
            }
        }
    }
}