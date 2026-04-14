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

        private CharacterIdentity _currentCharacterIdentity;
        private AsyncOperationHandle<IList<CharacterIdentity>> _loadHandle;
        private GameObject _currentCharacterInstance;

        PlayerManager _player;
        private Transform _playerContainer;

        private readonly Dictionary<string, CharacterIdentity> _characterMap = new();
        private readonly HashSet<string> _ownedCharacters = new();

        public AssetReferenceGameObject CurrentProjectilePrefab { get; private set; }
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
            EventBus.Subscribe(EventType.TransitionToLobby, DeActiveCurrentCharacter, 1);
            EventBus.Subscribe(EventType.Retry, DeActiveCurrentCharacter, 1);
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
                Debug.Log($"[CharacterManager] Loaded {_characterMap.Count} character identities.");

                // TODO: SaveManager에서 해금 목록 불러오기
                {
                    var playerData = PlayerManager.Instance.PlayerData;
                    _ownedCharacters.Add(playerData.currentCharacterName);
                }
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
        }

        public CharacterIdentity GetCurrentCharacterIdentity()
        {
            if (_currentCharacterIdentity != null)
                return _currentCharacterIdentity;

            _player ??= PlayerManager.Instance;

            if (!_characterMap.TryGetValue(_player.PlayerData.currentCharacterName, out var characterSO))
            {
                Debug.LogError($"[CharacterManager] CharacterSO not found for: {_player.PlayerData.currentCharacterName}");
                return null;
            }

            _currentCharacterIdentity = characterSO;
            return _currentCharacterIdentity;
        }

        public CharacterIdentity GetCharacterIdentityByName(string characterName)
        {
            _characterMap.TryGetValue(characterName, out var identity);
            return identity;
        }

        public void SyncCharacterIdentity(string characterName)
        {
            if (!_characterMap.TryGetValue(characterName, out var characterSO))
            {
                Debug.LogError($"[CharacterManager] CharacterSO not found for: {characterName}");
                return;
            }

            _currentCharacterIdentity = characterSO;
            _currentCharacterInstance = null;
        }

        public IReadOnlyDictionary<string, CharacterIdentity> GetCharacterMap() => _characterMap;

        public bool IsCharacterUnlocked(string characterName) => _ownedCharacters.Contains(characterName);

        public void UnlockCharacter(string characterName)
        {
            if (_ownedCharacters.Add(characterName))
            {
                Debug.Log($"[CharacterManager] Character unlocked: {characterName}");
            }
        }

        public bool TryLevelUpCharacter(string characterName)
        {
            if (!_characterMap.TryGetValue(characterName, out var identity))
                return false;

            var playerData = PlayerManager.Instance.PlayerData;
            int currentLevel = playerData.GetCharacterLevel(characterName);

            if (currentLevel >= identity.maxLevel)
                return false;

            int cost = identity.GetLevelUpCost(currentLevel);
            if (cost < 0 || !playerData.SpendGold(cost))
                return false;

            playerData.SetCharacterLevel(characterName, currentLevel + 1);
            Debug.Log($"[CharacterManager] '{characterName}' 레벨업: {currentLevel} → {currentLevel + 1}");
            return true;
        }
        
        public bool TryPurchaseCharacter(string characterName)
        {
            if (!_characterMap.TryGetValue(characterName, out var identity))
                return false;

            if (_ownedCharacters.Contains(characterName))
                return false;

            var playerData = PlayerManager.Instance.PlayerData;

            if (!playerData.SpendGold(identity.purchasePrice))
                return false;

            UnlockCharacter(characterName);
            playerData.SetCharacterLevel(characterName, 1);
            return true;
        }
    }
}
