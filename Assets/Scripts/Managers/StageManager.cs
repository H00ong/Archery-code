using UnityEngine;

public class StageManager : MonoBehaviour
{
    public EnemyManager EnemyManager { get; private set; }

    void Start()
    {
        EnemyManager = GetComponent<EnemyManager>();

        // EnemyManager.EnemyData = GameManager.Instance.DataManager.GetEnemyData();
    }

    void Update()
    {
        
    }

    public void LoadStage(int StageIndex)
    {
        
    }
}
