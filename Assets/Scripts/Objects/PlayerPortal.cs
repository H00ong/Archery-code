using UnityEngine;

public class PlayerPortal : MonoBehaviour
{
    private readonly string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) 
        {
            if (StageManager.CurrentState == StageState.Clear) 
            {
                StageManager.ChangeStageState(StageState.Combat);
            }
        }
    }
}
