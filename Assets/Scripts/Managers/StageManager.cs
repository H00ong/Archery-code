using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] MapManager mapManager;

    public static int currentStageIndex = 1;

    private void Start()
    {
        UpdateStage();
    }

    public void UpdateStage() 
    {   
        mapManager.GetNewMap();
        mapManager.SpawnEnemy();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!mapManager) mapManager = FindAnyObjectByType<MapManager>();
    }
#endif
}
