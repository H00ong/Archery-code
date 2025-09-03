using Mono.Cecil.Cil;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] MapManager mapManager;

    public static int currentStageIndex = 1;

    private void Start()
    {
        InitStage();
    }

    public void InitStage() 
    {
        currentStageIndex = 0;

        UpdateStage();
    }

    public void UpdateStage()
    {   
        currentStageIndex++;

        mapManager.GetNewMap();
        mapManager.PositionPlayer();
        mapManager.SpawnEnemy();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!mapManager) mapManager = FindAnyObjectByType<MapManager>();
    }
#endif
}
