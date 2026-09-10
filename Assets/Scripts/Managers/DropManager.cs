using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Managers
{
    /// <summary>
    /// 적 처치 시 지급되는 exp/gold 드랍 아이템/수치/확률을 한 곳에서 관리하는 싱글톤.
    /// EnemyIdentity/EnemyController마다 개별 등록할 필요 없이 이 매니저 하나만 씬에 두면 된다.
    /// </summary>
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance { get; private set; }

        [Header("Drop Prefabs")]
        [SerializeField] private AssetReferenceGameObject expItemPrefab;
        [SerializeField] private AssetReferenceGameObject goldItemPrefab;
        public AssetReferenceGameObject ExpItemPrefab => expItemPrefab;
        public AssetReferenceGameObject GoldItemPrefab => goldItemPrefab;

        [Header("Base Amount")]
        [SerializeField] private int baseGoldAmount = 10;

        [Header("Drop Chance (보스는 항상 드랍)")]
        [SerializeField, Range(0f, 1f)] private float expDropChance = 1f;
        [SerializeField, Range(0f, 1f)] private float goldDropChance = 0.7f;

        private const float RandomMin = 0.85f;
        private const float RandomMax = 1.15f;

        // LevelManager가 아직 없을 때만 쓰이는 폴백 값
        private const int FallbackExpAmount = 10;

        // 보스 처치 시 지급할 경험치 = 다음 레벨업까지 필요한 경험치의 비율(랜덤 없이 고정)
        private const float BossExpRatioOfRequired = 0.4f;
        private const float BossGoldMultiplier = 5f;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public bool TryGetExpDropAmount(bool isBoss, out int amount)
        {
            if (!isBoss && Random.value > expDropChance)
            {
                amount = 0;
                return false;
            }

            amount = GetExpDropAmount(isBoss);
            return true;
        }

        public bool TryGetGoldDropAmount(bool isBoss, out int amount)
        {
            if (!isBoss && Random.value > goldDropChance)
            {
                amount = 0;
                return false;
            }

            amount = GetGoldDropAmount(isBoss);
            return true;
        }

        private int GetExpDropAmount(bool isBoss)
        {
            var levelManager = LevelManager.Instance;
            if (levelManager == null) return FallbackExpAmount;

            if (isBoss)
                return Mathf.Max(1, Mathf.RoundToInt(levelManager.RequiredExp * BossExpRatioOfRequired));

            int perKill = levelManager.ExpPerKill;
            if (perKill <= 0) return 0;

            return Mathf.Max(1, Mathf.RoundToInt(perKill * Random.Range(RandomMin, RandomMax)));
        }

        private int GetGoldDropAmount(bool isBoss)
        {
            int goldAmount = GetBaseGoldAmount();

            if (isBoss)
                return Mathf.Max(1, Mathf.RoundToInt(goldAmount * BossGoldMultiplier));

            return Mathf.Max(1, Mathf.RoundToInt(goldAmount * Random.Range(RandomMin, RandomMax)));
        }

        private int GetBaseGoldAmount()
        {
            int mapGoldAmount = MapManager.Instance?.CurrentMapData?.goldAmount ?? 0;
            return mapGoldAmount > 0 ? mapGoldAmount : baseGoldAmount;
        }
    }
}
