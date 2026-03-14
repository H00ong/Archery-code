using System.Collections.Generic;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        [Header("Addressable Settings")]
        [SerializeField] private string characterLabel = "character_identity";

        private bool _isLoaded;
        private CharacterIdentity _currentCharacterIdentity;
        private AsyncOperationHandle<IList<CharacterIdentity>> _loadHandle;
        private GameObject _currentCharacterInstance;

        PlayerManager _player;
        private Transform _playerContainer;

        private readonly Dictionary<CharacterName, CharacterIdentity> _characterMap = new();
        public Transform PlayerContainer
        {
            get
            {
                if (_playerContainer != null)
                    return _playerContainer;

                var container = FindFirstObjectByType<PlayerContainer>()?.transform;

                if (container == null)
                {
                    container = new GameObject("Player Container").transform;
                    container.SetParent(null);
                    _playerContainer = container;
                }

                return container;
            }
        }
        
        public AssetReferenceGameObject CurrentProjectilePrefab { get; private set; }

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

        void OnEnable()
        {
            EventBus.Subscribe(EventType.TransitionToLobby, DeActiveCurrentCharacter);
            EventBus.Subscribe(EventType.Retry, DeActiveCurrentCharacter);
        }

        private void OnDisable()
        {
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);

            EventBus.Unsubscribe(EventType.TransitionToLobby, DeActiveCurrentCharacter);
            EventBus.Unsubscribe(EventType.Retry, DeActiveCurrentCharacter);
        }


        public async Awaitable LoadCharacterIdentitiesAsync()
        {
            _loadHandle = Addressables.LoadAssetsAsync<CharacterIdentity>(characterLabel, null);
            await _loadHandle.Task;
            destroyCancellationToken.ThrowIfCancellationRequested();

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _characterMap.Clear();

                foreach (var so in _loadHandle.Result)
                {
                    if (so && !_characterMap.TryAdd(so.characterName, so))
                    {
                        Debug.LogWarning($"[CharacterManager] Duplicate CharacterName: {so.characterName}");
                    }
                }

                _isLoaded = true;
                Debug.Log($"[CharacterManager] Loaded {_characterMap.Count} character identities.");
            }
            else
            {
                Debug.LogError("[CharacterManager] Failed to load character identities.");
                throw new System.InvalidOperationException("Failed to load character identities.");
            }
        }

        public async Awaitable LoadAndSpawnCharacterAsync()
        {
            if (_currentCharacterIdentity == null)
            {
                _currentCharacterIdentity = GetCurrentCharacterIdentity();
            }
            
            if(_currentCharacterInstance != null)
                return;

            GameObject go = null;
            var pool = PoolManager.Instance;

            if (!pool.TryGetObject(_currentCharacterIdentity.characterPrefab, out go, PlayerContainer))
            {
                go = await pool.GetObjectAsync(_currentCharacterIdentity.characterPrefab, PlayerContainer, 0);
            }

            destroyCancellationToken.ThrowIfCancellationRequested();

            if (go == null)
            {
                Debug.LogError($"[CharacterManager] Failed to spawn character: {_currentCharacterIdentity.characterName}");
                throw new System.InvalidOperationException($"Failed to spawn character: {_currentCharacterIdentity.characterName}");
            }

            CurrentProjectilePrefab = _currentCharacterIdentity.projectilePrefab;
            _currentCharacterInstance = go;
            _currentCharacterInstance.transform.localPosition = Vector3.zero;

            var playerController = _currentCharacterInstance.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("[CharacterManager] PlayerController not found on spawned character prefab.");
                throw new System.InvalidOperationException("PlayerController not found on spawned character prefab.");
            }

            playerController.InitModule();
            playerController.Stat.ApplyBaseStat(_currentCharacterIdentity.baseStat);

            EventBus.Publish(EventType.PlayerSpawned);
            
            Debug.Log($"[CharacterManager] Character spawned and stats applied: {_currentCharacterIdentity.characterName}");
        }

        public void DeActiveCurrentCharacter()
        {
            if (_currentCharacterInstance == null)
                return;

            PoolManager.Instance.ReturnObject(_currentCharacterInstance);
            _currentCharacterInstance = null;
        }

        public CharacterIdentity GetCurrentCharacterIdentity()
        {
            if (_currentCharacterIdentity != null)
                return _currentCharacterIdentity;

            _player ??= PlayerManager.Instance;

            if (!_characterMap.TryGetValue(_player.PlayerData.characterName, out var characterSO))
            {
                Debug.LogError($"[CharacterManager] CharacterSO not found for: {_player.PlayerData.characterName}");
                return null;
            }

            _currentCharacterIdentity = characterSO;
            return _currentCharacterIdentity;
        }

        public void SyncCharacterIdentity(CharacterName characterName)
        {
            if (!_characterMap.TryGetValue(characterName, out var characterSO))
            {
                Debug.LogError($"[CharacterManager] CharacterSO not found for: {characterName}");
                return;
            }

            _currentCharacterIdentity = characterSO;
        }
    }
}
