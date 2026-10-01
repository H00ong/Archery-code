#if UNITY_EDITOR
using System.Linq;
using Enemy;
using UnityEditor;
using UnityEngine;

public static class EnemyHealthBarBatchAdder
{
    private const string SliderCanvasPath = "Assets/Prefabs/UI/Enemy Slider Canvas.prefab";
    private const string EnemyFolder = "Assets/Prefabs/Enemies";
    private const string BossFolderName = "Boss Enemies";
    private const float BossSliderScale = 1.3f;
    private const float BossSliderWidthScale = 1.25f;

    [MenuItem("Tools/Enemy/Add HealthBar To All Enemy Prefabs")]
    public static void AddToAll()
    {
        var canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SliderCanvasPath);
        if (canvasPrefab == null)
        {
            Debug.LogError($"[EnemyHealthBarBatchAdder] 프리팹 없음: {SliderCanvasPath}");
            return;
        }

        var paths = AssetDatabase.FindAssets("t:Prefab", new[] { EnemyFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

        int added = 0, linked = 0, skipped = 0;

        try
        {
            foreach (var path in paths)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var controller = root.GetComponentInChildren<EnemyController>(true);
                    if (controller == null) { skipped++; continue; }

                    var bar = root.GetComponentInChildren<EnemyHealthBar>(true);
                    bool changed = false;

                    if (bar == null)
                    {
                        var canvas = (GameObject)PrefabUtility.InstantiatePrefab(canvasPrefab, root.transform);
                        canvas.transform.localPosition = Vector3.zero;
                        canvas.transform.localRotation = Quaternion.identity;

                        bar = canvas.GetComponentInChildren<EnemyHealthBar>(true);

                        if (path.Contains($"/{BossFolderName}/"))
                            bar.transform.localScale = Vector3.Scale(
                                bar.transform.localScale,
                                new Vector3(BossSliderScale * BossSliderWidthScale, BossSliderScale, BossSliderScale));

                        added++;
                        changed = true;
                    }

                    if (controller.healthBar != bar)
                    {
                        controller.healthBar = bar;
                        linked++;
                        changed = true;
                    }

                    if (changed)
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    else
                        skipped++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"[EnemyHealthBarBatchAdder] 추가 {added}, 연결 {linked}, 건너뜀 {skipped} (총 {paths.Length})");
    }
}
#endif
