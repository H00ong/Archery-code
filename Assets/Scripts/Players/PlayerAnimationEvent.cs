using Game.Player;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [SerializeField] PlayerAttack playerAttack;

    private void Start()
    {
         if(playerAttack == null) 
            playerAttack = GetComponent<PlayerAttack>();
    }

    public void Shoot() 
    {
        if (PlayerManager.CurrentState == PlayerState.Attack)
            playerAttack.Shoot();
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerAttack == null) playerAttack = GetComponent<PlayerAttack>();  
    }
#endif
}
