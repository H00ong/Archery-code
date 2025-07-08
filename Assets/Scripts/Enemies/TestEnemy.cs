using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public string enemyName = "Slime";
    private float moveSpeed;

    void Start()
    {
        var data = DataManager.Instance.GetEnemyData(enemyName);

        if (data != null)
        {
            moveSpeed = data.moveSpeed;

            Debug.Log(moveSpeed + "Congratulations");
        }
    }

    void Update()
    {
        
    }
}
