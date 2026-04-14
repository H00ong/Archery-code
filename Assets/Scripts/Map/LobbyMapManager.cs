using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class LobbyMapManager : MonoBehaviour
    {
        public static LobbyMapManager Instance { get; private set; }

        [Header("Map Placement Settings")]
        [SerializeField] private float yOffset = 30f;
        [SerializeField] private Transform mapParent;

        [Header("Locked Visual")]
        [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.3f;

        private readonly List<GameObject> _dummyMaps = new();
        private readonly List<AssetReferenceGameObject> _dummyRefs = new();
        private readonly Dictionary<int, Color> _originalBaseColors = new();   // "_Color" 원본
        private readonly Dictionary<int, Color> _originalUrpColors = new();    // "_BaseColor" 원본

        private DataManager _dataManager;
        private MapManager _mapManager;
        private PoolManager _poolManager;
        private LobbyCameraController _lobbyCameraController;

        private Transform _dummyMapPool;
        private bool _initialized;
        private int _firstUnlockedMapIndex;

        public int FocusedMapIndex => _lobbyCameraController != null
            ? _lobbyCameraController.CurrentStageIndex
            : 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe(EventType.LobbySceneLoaded, OnLobbySceneLoaded);
            EventBus.Subscribe(EventType.MapCleared, OnNewMapCleared, 1);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LobbySceneLoaded, OnLobbySceneLoaded);
            EventBus.Unsubscribe(EventType.MapCleared, OnNewMapCleared);
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F3))
            {
                SelectFocusedMap();
            }
        }

        private void OnLobbySceneLoaded()
        {
            if (!_initialized)
                return;

            SetDummyPoolActive(true);
            SetLobbyCamera();
        }

        public async Awaitable InitLobbyAsync()
        {
            _dataManager = DataManager.Instance;
            _mapManager = MapManager.Instance;
            _poolManager = PoolManager.Instance;

            if (_dummyRefs.Count == 0)
                CacheDummyRefs();

            await BuildLobbyMapsAsync();

            _initialized = true;
        }

        private void SetLobbyCamera()
        {
            _lobbyCameraController = FindAnyObjectByType<LobbyCameraController>();

            if (_lobbyCameraController != null)
                _lobbyCameraController.Setup(_dummyMaps.Count, yOffset, _firstUnlockedMapIndex, _dummyMaps[0].transform.position);
        }

        private void CacheDummyRefs()
        {
            int mapCount = _dataManager.GetMapCount();

            for (int i = 0; i < mapCount; i++)
            {
                var mapData = _dataManager.GetMapData(i);
                if (mapData == null) continue;

                var mapSo = _mapManager.GetMapScriptable(mapData.mapId);
                if (mapSo == null || mapSo.lobbyDummyMap == null)
                {
                    Debug.LogWarning($"[LobbyMapController] lobbyMapDummy 없음 — mapId: {mapData.mapId}");
                    _dummyRefs.Add(null);
                    continue;
                }

                _dummyRefs.Add(mapSo.lobbyDummyMap);
            }
        }

        private async Awaitable BuildLobbyMapsAsync()
        {
            SetNextMapIndex();
            _dummyMapPool ??= _poolManager.lobbyMapPool;
            _dummyMapPool.gameObject.SetActive(false);

            for (int i = 0; i < _dummyRefs.Count; i++)
            {
                var dummyRef = _dummyRefs[i];
                if (dummyRef == null)
                    continue;

                GameObject map;
                
                if (!_poolManager.TryGetObject(dummyRef, out map, _dummyMapPool))
                {
                    map = await _poolManager.GetObjectAsync(dummyRef, _dummyMapPool, extraPrewarmCount: 0);
                }

                if (map == null)
                {
                    Debug.LogError($"[LobbyMapController] 더미 풀링 실패: index {i}");
                    continue;
                }

                map.transform.position = new Vector3(0f, i * yOffset, 0f);
                map.SetActive(true);

                bool isLocked = i > _firstUnlockedMapIndex;
                ApplyVisual(map, isLocked);

                _dummyMaps.Add(map);
            }
        }

        private void ApplyVisual(GameObject mapObj, bool locked)
        {
            var renderers = mapObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    int id = mat.GetInstanceID();

                    if (mat.HasProperty("_Color"))
                    {
                        if (!_originalBaseColors.ContainsKey(id))
                            _originalBaseColors[id] = mat.color;

                        var c = _originalBaseColors[id];
                        mat.color = locked ? new Color(c.r, c.g, c.b, lockedAlpha) : c;
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        if (!_originalUrpColors.ContainsKey(id))
                            _originalUrpColors[id] = mat.GetColor("_BaseColor");
                        var c = _originalUrpColors[id];
                        mat.SetColor("_BaseColor", locked ? new Color(c.r, c.g, c.b, lockedAlpha) : c);
                    }
                }
            }
        }

        private void SetNextMapIndex()
        {
            _firstUnlockedMapIndex = MapManager.Instance.MaxMapIndex + 1;
        }

        private void SetDummyPoolActive(bool active)
        {
            _dummyMapPool?.gameObject.SetActive(active);
        }

        private void OnNewMapCleared()
        {
            if (_firstUnlockedMapIndex >= _dummyMaps.Count)
                return;

            var go = _dummyMaps[_firstUnlockedMapIndex];
            ApplyVisual(go, locked : false);
            SetNextMapIndex();
        }

        public void SelectFocusedMap()
        {
            int index = FocusedMapIndex;

            if (index > _firstUnlockedMapIndex)
            {
                Debug.LogWarning($"[LobbyMapController] 잠긴 맵은 선택할 수 없습니다: index {index}");
                return;
            }

            _mapManager.SetCurrentMapIndex(index);
            GameManager.Instance.ChangeScene(SceneState.InGame);
            SetDummyPoolActive(false);
        }
    }
}
