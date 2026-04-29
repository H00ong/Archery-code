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

        private struct LobbyCharacterEntry
        {
            public string name;
            public AssetReferenceGameObject dummyRef;
            public GameObject dummyInstance;
        }

        private readonly List<LobbyCharacterEntry> _characters = new();
        private readonly Dictionary<int, Color> _originalBaseColors = new();   // "_Color" 원본
        private readonly Dictionary<int, Color> _originalUrpColors = new();    // "_BaseColor" 원본

        private Transform _dummyCharacterPool;
        private bool _initialized;
        private int _currentIndex;
        private int _equippedIndex;

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

            if (_characters.Count == 0)
                CacheDummyRefs();

            await BuildLobbyCharactersAsync();

            // 현재 선택된 캐릭터 인덱스로 초기화
            string currentName = PlayerManager.Instance.PlayerData.currentCharacterName;
            _currentIndex = FindIndexByName(currentName);
            _equippedIndex = _currentIndex;

            _initialized = true;
        }

        private int FindIndexByName(string name)
        {
            for (int i = 0; i < _characters.Count; i++)
            {
                if (_characters[i].name == name)
                    return i;
            }
            return 0;
        }

        private void CacheDummyRefs()
        {
            var characterMap = _characterManager.GetCharacterMap();

            foreach (var kvp in characterMap)
            {
                var identity = kvp.Value;
                if (identity.lobbyCharacterDummy == null)
                {
                    Debug.LogWarning($"[LobbyCharacterManager] lobbyCharacterDummy 없음 — character: {kvp.Key}");
                    continue;
                }

                _characters.Add(new LobbyCharacterEntry
                {
                    name = kvp.Key,
                    dummyRef = identity.lobbyCharacterDummy,
                    dummyInstance = null,
                });
            }

            // 정렬 (index 기준)
            _characters.Sort((a, b) =>
            {
                var indexA = characterMap[a.name];
                var indexB = characterMap[b.name];
                return indexA.index.CompareTo(indexB.index);
            });
        }

        private async Awaitable BuildLobbyCharactersAsync()
        {
            _dummyCharacterPool ??= _poolManager.lobbyCharacterPool;
            _dummyCharacterPool.gameObject.SetActive(false);

            for (int i = 0; i < _characters.Count; i++)
            {
                var entry = _characters[i];
                if (entry.dummyInstance != null)
                    continue;

                GameObject dummy;

                if (!_poolManager.TryGetObject(entry.dummyRef, out dummy, _dummyCharacterPool))
                {
                    dummy = await _poolManager.GetObjectAsync(entry.dummyRef, _dummyCharacterPool, extraPrewarmCount: 0);
                }

                if (dummy == null)
                {
                    Debug.LogError($"[LobbyCharacterManager] 더미 풀링 실패: {entry.name}");
                    continue;
                }

                dummy.transform.position = dummyPosition;
                dummy.SetActive(false);

                entry.dummyInstance = dummy;
                _characters[i] = entry;
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
            SetLastValidState();
        }

        public void SetLastValidState()
        {
            ChangeCharacterToEquipped();
            _currentIndex = _equippedIndex;
        }

        public void ChangeCharacter(int direction)
        {
            int newIndex = _currentIndex + direction;
            newIndex = Mathf.Clamp(newIndex, 0, _characters.Count - 1);

            if (newIndex == _currentIndex)
                return;

            _characters[_currentIndex].dummyInstance.SetActive(false);
            _currentIndex = newIndex;
            _characters[_currentIndex].dummyInstance.SetActive(true);
            ApplyVisual(_characters[_currentIndex].dummyInstance, _characters[_currentIndex].name);
        }

        public void ChangeCharacterToEquipped()
        {
            _characters[_currentIndex].dummyInstance.SetActive(false);
            _characters[_equippedIndex].dummyInstance.SetActive(true);
        }

        public void SelectCurrent()
        {
            if (_currentIndex < 0 || _currentIndex >= _characters.Count)
                return;

            string name = _characters[_currentIndex].name;
            if (!_characterManager.IsCharacterUnlocked(name))
            {
                Debug.Log($"[LobbyCharacterManager] 잠긴 캐릭터는 선택할 수 없습니다: {name}");
                return;
            }

            _characterManager.SyncCharacterIdentity(name);
            _equippedIndex = _currentIndex;
            Debug.Log($"[LobbyCharacterManager] 캐릭터 선택: {name}");
        }

        public string GetCurrentCharacterName()
        {
            if (_currentIndex >= 0 && _currentIndex < _characters.Count)
                return _characters[_currentIndex].name;

            return string.Empty;
        }

        private void ShowOnlyCurrent()
        {
            _characters[_currentIndex].dummyInstance.SetActive(true);
            ApplyVisual(_characters[_currentIndex].dummyInstance, _characters[_currentIndex].name);
        }

        private void HideAll()
        {
            foreach (var entry in _characters)
                entry.dummyInstance.SetActive(false);
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

        public bool IsCurrentCharacterLocked()
        {
            if (_currentIndex < 0 || _currentIndex >= _characters.Count)
                return true;

            return !_characterManager.IsCharacterUnlocked(_characters[_currentIndex].name);
        }

        public bool IsCurrentCharacterEquipped()
        {
            if (_currentIndex < 0 || _currentIndex >= _characters.Count)
                return false;

            string equippedName = PlayerManager.Instance.PlayerData.currentCharacterName;
            return _characters[_currentIndex].name == equippedName;
        }

        public CharacterIdentity GetCurrentCharacterIdentity()
        {
            if (_currentIndex < 0 || _currentIndex >= _characters.Count)
                return null;

            return _characterManager.GetCharacterIdentityByName(_characters[_currentIndex].name);
        }
    }
}
