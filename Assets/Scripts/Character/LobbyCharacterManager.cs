using System.Collections.Generic;
using Managers;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class LobbyCharacterManager : MonoBehaviour
    {
        public static LobbyCharacterManager Instance { get; private set; }

        [Header("Character Placement")]
        [SerializeField] private Vector3 dummyPosition = new Vector3(-100f, 0f, 0f);

        [Header("Locked Visual")]
        [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.3f;

        private readonly List<AssetReferenceGameObject> _dummyRefs = new();
        private readonly List<GameObject> _dummyCharacters = new();
        private readonly List<string> _characterNames = new();
        private readonly Dictionary<int, Color> _originalColors = new();

        private Transform _dummyCharacterPool;
        private bool _initialized;
        private int _currentIndex;
        private int _lastSelectedIndex;


        private CharacterManager _characterManager;
        private PoolManager _poolManager;

        public Vector3 DummyPosition => dummyPosition;

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
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(EventType.LobbySceneLoaded, OnLobbySceneLoaded);
        }

        private void OnLobbySceneLoaded()
        {
            if (!_initialized)
                return;

            SetDummyPoolActive(false);
        }

        public async Awaitable InitLobbyAsync()
        {
            _characterManager = CharacterManager.Instance;
            _poolManager = PoolManager.Instance;

            if (_dummyRefs.Count == 0)
                CacheDummyRefs();

            await BuildLobbyCharactersAsync();

            // 현재 선택된 캐릭터 인덱스로 초기화
            string currentName = DataManager.Instance.GetPlayerData().characterName;
            int idx = _characterNames.IndexOf(currentName);
            _currentIndex = idx >= 0 ? idx : 0;
            _lastSelectedIndex = _currentIndex;

            _initialized = true;
        }

        private void CacheDummyRefs()
        {
            var characterMap = _characterManager.GetCharacterMap();

            foreach (var kvp in characterMap)
            {
                var identity = kvp.Value;
                if (identity.lobbyCharacterDummy == null)
                {
                    Debug.LogWarning($"[LobbyCharacterController] lobbyCharacterDummy 없음 — character: {kvp.Key}");
                    continue;
                }

                _dummyRefs.Add(identity.lobbyCharacterDummy);
                _characterNames.Add(kvp.Key);
            }
        }

        private async Awaitable BuildLobbyCharactersAsync()
        {
            _dummyCharacterPool ??= _poolManager.lobbyCharacterPool;
            _dummyCharacterPool.gameObject.SetActive(false);

            for (int i = 0; i < _dummyRefs.Count; i++)
            {
                var dummyRef = _dummyRefs[i];

                GameObject dummy;

                if (!_poolManager.TryGetObject(dummyRef, out dummy, _dummyCharacterPool))
                {
                    dummy = await _poolManager.GetObjectAsync(dummyRef, _dummyCharacterPool, extraPrewarmCount: 0);
                }

                if (dummy == null)
                {
                    Debug.LogError($"[LobbyCharacterController] 더미 풀링 실패: {_characterNames[i]}");
                    continue;
                }

                dummy.transform.position = dummyPosition;
                dummy.SetActive(false);

                _dummyCharacters.Add(dummy);
            }
        }

        public void Show()
        {
            SetDummyPoolActive(true);
            ShowOnlyCurrent();
        }

        public void Hide()
        {
            HideAll();
            SetDummyPoolActive(false);

            _currentIndex = _lastSelectedIndex;
        }

        public void ChangeCharacter(int direction)
        {
            int newIndex = _currentIndex + direction;
            newIndex = Mathf.Clamp(newIndex, 0, _dummyCharacters.Count - 1);

            if (newIndex == _currentIndex) return;

            _dummyCharacters[_currentIndex].SetActive(false);
            _currentIndex = newIndex;
            _dummyCharacters[_currentIndex].SetActive(true);
            ApplyVisual(_dummyCharacters[_currentIndex], _characterNames[_currentIndex]);
        }

        public void SelectCurrent()
        {
            if (_currentIndex < 0 || _currentIndex >= _characterNames.Count)
                return;

            string name = _characterNames[_currentIndex];
            if (!_characterManager.IsCharacterUnlocked(name))
            {
                Debug.Log($"[LobbyCharacterManager] 잠긴 캐릭터는 선택할 수 없습니다: {name}");
                return;
            }

            _characterManager.SyncCharacterIdentity(name);
            _lastSelectedIndex = _currentIndex;
            Debug.Log($"[LobbyCharacterManager] 캐릭터 선택: {name}");
            return;
        }

        public string GetCurrentCharacterName()
        {
            if (_currentIndex >= 0 && _currentIndex < _characterNames.Count)
                return _characterNames[_currentIndex];
            return string.Empty;
        }

        private void ShowOnlyCurrent()
        {
            _dummyCharacters[_currentIndex].SetActive(true);
            ApplyVisual(_dummyCharacters[_currentIndex], _characterNames[_currentIndex]);
        }

        private void HideAll()
        {
            foreach (var dummy in _dummyCharacters)
                dummy.SetActive(false);
        }

        private void SetDummyPoolActive(bool active)
        {
            _dummyCharacterPool?.gameObject.SetActive(active);
        }

        private void ApplyVisual(GameObject obj, string characterName)
        {
            bool locked = !_characterManager.IsCharacterUnlocked(characterName);
            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    int id = mat.GetInstanceID();

                    if (mat.HasProperty("_Color"))
                    {
                        if (!_originalColors.ContainsKey(id))
                            _originalColors[id] = mat.color;

                        var c = _originalColors[id];
                        mat.color = locked ? new Color(c.r, c.g, c.b, lockedAlpha) : c;
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        int baseId = id + 1;
                        if (!_originalColors.ContainsKey(baseId))
                            _originalColors[baseId] = mat.GetColor("_BaseColor");

                        var c = _originalColors[baseId];
                        mat.SetColor("_BaseColor", locked ? new Color(c.r, c.g, c.b, lockedAlpha) : c);
                    }
                }
            }
        }

        public bool IsCurrentCharacterLocked()
        {
            if (_currentIndex < 0 || _currentIndex >= _characterNames.Count)
                return true;

            return !_characterManager.IsCharacterUnlocked(_characterNames[_currentIndex]);
        }
    }
}
