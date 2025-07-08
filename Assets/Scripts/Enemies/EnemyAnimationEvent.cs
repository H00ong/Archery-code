using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    void AnimationTrigger() 
    {
        enemy.AnimationTrigger();
    }
}
