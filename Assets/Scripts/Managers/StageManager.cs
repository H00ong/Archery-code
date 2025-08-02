using UnityEngine;

public class StageManager : MonoBehaviour
{
    public EnemyManager EnemyManager { get; private set; }
    public PoolManager PoolManager { get; private set; }

    void Start()
    {
        EnemyManager = GetComponent<EnemyManager>();
        PoolManager = GetComponent<PoolManager>();
        // EnemyManager.EnemyData = GameManager.Instance.DataManager.GetEnemyData();
    }

    void Update()
    {
        
    }

    public void LoadStage(int StageIndex)
    {
        
    }
}
