using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// DDL 객체. 로비 진입 시 PoolManager로 맵 더미를 생성·배치하고,
    /// InGame 전환 시 전부 ReturnPool한다.
    /// 궁수의 전설2 스타일: 모든 맵을 보여주되, 잠긴 맵은 회색으로 표시.
    /// </summary>
    public class LobbyMapController : MonoBehaviour
    {
        public static LobbyMapController Instance { get; private set; }

        [Header("Map Placement Settings")]
        [SerializeField] private float yOffset = 30f;
        [SerializeField] private Transform mapParent;

        [Header("Locked Color Tint")]
        [SerializeField] private Color lockedTint = new Color(0.35f, 0.35f, 0.35f, 1f);

        private readonly List<GameObject> _dummyMaps = new();
        private readonly List<AssetReferenceGameObject> _dummyRefs = new();
        private readonly Dictionary<int, Color> _originalColors = new();

        private DataManager _dataManager;
        private MapManager _mapManager;
        private PoolManager _poolManager;
        private LobbyCameraController _lobbyCameraController;

        private Transform _dummyMapPool;
        private bool _initialized;
        private int _nextMapIndex;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe(EventType.MapCleared, OnNewMapCleared, 1);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus.Unsubscribe(EventType.MapCleared, OnNewMapCleared);
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F3))
            {
                SelectFocusedMap();
            }
        }

        private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_initialized)
                return;

            if (scene.name != "Lobby")
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
                _lobbyCameraController.Setup(_dummyMaps.Count, yOffset, _nextMapIndex, _dummyMaps[0].transform.position);
        }

        private void CacheDummyRefs()
        {
            int mapCount = _dataManager.GetMapCount();
            for (int i = 0; i < mapCount; i++)
            {
                var mapData = _dataManager.GetMapData(i);
                if (mapData == null) continue;

                var mapSo = _mapManager.GetMapScriptable(mapData.mapId);
                if (mapSo == null || mapSo.lobbyMapDummy == null)
                {
                    Debug.LogWarning($"[LobbyMapController] lobbyMapDummy 없음 — mapId: {mapData.mapId}");
                    _dummyRefs.Add(null);
                    continue;
                }

                _dummyRefs.Add(mapSo.lobbyMapDummy);
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

                GameObject dummy;
                
                if (!_poolManager.TryGetObject(dummyRef, out dummy, _dummyMapPool))
                {
                    dummy = await _poolManager.GetObjectAsync(dummyRef, _dummyMapPool, extraPrewarmCount: 0);
                }

                if (dummy == null)
                {
                    Debug.LogError($"[LobbyMapController] 더미 풀링 실패: index {i}");
                    continue;
                }

                dummy.transform.position = new Vector3(0f, i * yOffset, 0f);
                dummy.SetActive(true);

                bool isLocked = i > _nextMapIndex;
                if (isLocked)
                    ApplyLockedVisual(dummy);

                _dummyMaps.Add(dummy);
            }
        }

        private void ApplyLockedVisual(GameObject mapObj)
        {
            var renderers = mapObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    int id = mat.GetInstanceID();

                    if (mat.HasProperty("_Color"))
                    {
                        if (!_originalColors.ContainsKey(id))
                            _originalColors[id] = mat.color;
                        mat.color = _originalColors[id] * lockedTint;
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        int baseId = id + 1;
                        if (!_originalColors.ContainsKey(baseId))
                            _originalColors[baseId] = mat.GetColor("_BaseColor");
                        mat.SetColor("_BaseColor", _originalColors[baseId] * lockedTint);
                    }
                }
            }
        }

        private void ApplyOriginalVisual(GameObject mapObj)
        {
            var renderers = mapObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    int id = mat.GetInstanceID();

                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = _originalColors.TryGetValue(id, out var origColor)
                            ? origColor
                            : Color.white;
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        int baseId = id + 1;
                        mat.SetColor("_BaseColor",
                            _originalColors.TryGetValue(baseId, out var origBase)
                                ? origBase
                                : Color.white);
                    }
                }
            }
        }

        private void SetNextMapIndex()
        {
            _nextMapIndex = MapManager.Instance.MaxMapIndex + 1;
        }

        private void SetDummyPoolActive(bool active)
        {
            _dummyMapPool?.gameObject.SetActive(active);
        }

        private void OnNewMapCleared()
        {
            if (_nextMapIndex >= _dummyMaps.Count)
                return;

            var go = _dummyMaps[_nextMapIndex];
            ApplyOriginalVisual(go);
            SetNextMapIndex();
        }

        public void SelectFocusedMap()
        {
            int index = FocusedMapIndex;
            if (index > _nextMapIndex)
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
