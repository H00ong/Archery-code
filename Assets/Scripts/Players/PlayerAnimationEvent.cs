using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] Transform shootingPosition;

    public void Shoot() 
    {
        if(PlayerManager.CurrentState == PlayerState.Attack)
            playerAttack.Shoot(shootingPosition.position);
    } 
}
