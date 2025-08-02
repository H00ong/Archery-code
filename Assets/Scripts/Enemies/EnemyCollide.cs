using UnityEngine;

public class EnemyCollide : MonoBehaviour
{
    Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();


    }

}
