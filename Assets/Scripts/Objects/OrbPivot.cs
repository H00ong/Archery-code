using Players;
using UnityEditor;
using UnityEngine;

public class OrbPivot : MonoBehaviour
{
    private Vector3 _pivotPos;

    private void Update()
    {
        if (PlayerController.Instance == null)
            return;

        PlayerController player = PlayerController.Instance;
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        _pivotPos = playerRigidbody.position;
        
        transform.position = new Vector3(_pivotPos.x, transform.position.y, _pivotPos.z);
    }
}
