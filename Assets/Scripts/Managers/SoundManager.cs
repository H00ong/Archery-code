using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SfxType
{
    PlayerShoot,
    PlayerHit,
    PlayerDie,
    LevelUp,
    BarrelPickup,
    MeteorImpact,
    GoldPickup,
    ExpPickup,
    EnemyMelee,
    EnemyShoot,
    EnemyFlyingShoot,
    EnemyHit,
    EnemyDie,
    BossDie,
}

/// <summary>
/// BGM/SFX 재생 담당 (DontDestroyOnLoad). 클립은 Resources/Sounds/{BGM,SFX}/ 에서 이름으로 로드한다.
/// SFX 파일명 = SfxType 이름, 맵 BGM 파일명 = "Map_" + mapData.json의 mapId.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const string BgmPath = "Sounds/BGM/";
    private const string SfxPath = "Sounds/SFX/";
    private const string LoadingBgm = "Loading";
    private const string LobbyBgm = "Lobby";
    private const string MapBgmPrefix = "Map_";
    private const float BgmFadeDuration = 0.6f;

    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Tooltip("슬라이더 100%일 때 실제 BGM 볼륨. 효과음이 묻히지 않도록 낮게 둔다.")]
    [Range(0f, 1f)] [SerializeField] private float bgmMaxVolume = 0.45f;

    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    // (기본 볼륨, 같은 효과음의 최소 재생 간격 초) — 다수의 적/투사체가 같은 프레임에 소리를 내도 과부하·소음이 되지 않게 한다.
    private static readonly Dictionary<SfxType, (float volume, float minInterval)> SfxSettings = new()
    {
        { SfxType.PlayerShoot,      (0.45f, 0.05f) },
        { SfxType.PlayerHit,        (0.8f,  0.12f) },
        { SfxType.PlayerDie,        (0.9f,  0.5f) },
        { SfxType.LevelUp,          (0.8f,  0.2f) },
        { SfxType.BarrelPickup,     (0.7f,  0.1f) },
        { SfxType.MeteorImpact,     (0.65f, 0.08f) },
        { SfxType.GoldPickup,       (0.45f, 0.05f) },
        { SfxType.ExpPickup,        (0.35f, 0.04f) },
        { SfxType.EnemyMelee,       (0.45f, 0.1f) },
        { SfxType.EnemyShoot,       (0.4f,  0.08f) },
        { SfxType.EnemyFlyingShoot, (0.45f, 0.1f) },
        { SfxType.EnemyHit,         (0.4f,  0.05f) },
        { SfxType.EnemyDie,         (0.55f, 0.06f) },
        { SfxType.BossDie,          (1f,    0.5f) },
    };

    private readonly Dictionary<SfxType, AudioClip> _sfxClips = new();
    private readonly Dictionary<SfxType, float> _lastPlayTime = new();

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private Coroutine _bgmRoutine;
    private float _bgmFade = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateSources();
            LoadSfxClips();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EventBus.Subscribe(EventType.LevelUp, OnLevelUp);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EventBus.Unsubscribe(EventType.LevelUp, OnLevelUp);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        var save = SaveSystem.SaveManager.Instance?.CurrentData;
        if (save != null)
        {
            bgmVolume = Mathf.Clamp01(save.bgmVolume);
            sfxVolume = Mathf.Clamp01(save.sfxVolume);
        }

        ApplyVolumes();
        PlayBgmForScene(SceneManager.GetActiveScene().name);
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        EventBus.Publish(EventType.SettingsChanged);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        PlaySfx(SfxType.GoldPickup); // 슬라이더 조절 중 크기를 바로 들어볼 수 있게 하는 미리듣기
        EventBus.Publish(EventType.SettingsChanged);
    }

    // ─────────────────────────────────────────────────────
    // SFX
    // ─────────────────────────────────────────────────────

    /// <summary> 어디서든 호출 가능한 단축 진입점. SoundManager가 없으면 무시된다. </summary>
    public static void Play(SfxType type)
    {
        if (Instance != null) Instance.PlaySfx(type);
    }

    public void PlaySfx(SfxType type)
    {
        if (_sfxSource == null || !_sfxClips.TryGetValue(type, out var clip)) return;

        var (volume, minInterval) = SfxSettings[type];
        float now = Time.unscaledTime;
        if (_lastPlayTime.TryGetValue(type, out float last) && now - last < minInterval) return;

        _lastPlayTime[type] = now;
        _sfxSource.PlayOneShot(clip, volume);
    }

    private void OnLevelUp() => PlaySfx(SfxType.LevelUp);

    // ─────────────────────────────────────────────────────
    // BGM
    // ─────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlayBgmForScene(scene.name);

    private void PlayBgmForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Loading":
                PlayBgm(LoadingBgm);
                break;
            case "Lobby":
                PlayBgm(LobbyBgm);
                break;
            case "InGame":
                PlayMapBgm();
                break;
        }
    }

    private void PlayMapBgm()
    {
        var mapManager = MapManager.Instance;
        var mapData = mapManager != null && DataManager.Instance != null
            ? DataManager.Instance.GetMapData(mapManager.CurrentMapIndex)
            : null;

        if (mapData == null)
        {
            Debug.LogWarning("[SoundManager] 현재 맵 데이터를 찾지 못해 맵 BGM을 재생하지 않습니다.");
            return;
        }

        PlayBgm(MapBgmPrefix + mapData.mapId);
    }

    public void PlayBgm(string clipName)
    {
        if (_bgmSource == null) return;

        var clip = Resources.Load<AudioClip>(BgmPath + clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] BGM 클립 없음: Resources/{BgmPath}{clipName}");
            return;
        }

        // Retry 등으로 같은 씬을 다시 로드하면 곡을 처음부터 다시 틀지 않는다.
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

        if (_bgmRoutine != null) StopCoroutine(_bgmRoutine);
        _bgmRoutine = StartCoroutine(SwitchBgm(clip));
    }

    private IEnumerator SwitchBgm(AudioClip next)
    {
        if (_bgmSource.isPlaying)
            yield return FadeBgm(0f);

        var prev = _bgmSource.clip;
        _bgmSource.clip = next;
        _bgmSource.Play();

        if (prev != null && prev != next)
            Resources.UnloadAsset(prev);

        yield return FadeBgm(1f);
        _bgmRoutine = null;
    }

    // timeScale=0(일시정지/레벨업) 중에도 진행되도록 unscaled 시간을 쓴다.
    private IEnumerator FadeBgm(float target)
    {
        float start = _bgmFade;
        float t = 0f;
        while (t < BgmFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            _bgmFade = Mathf.Lerp(start, target, t / BgmFadeDuration);
            ApplyVolumes();
            yield return null;
        }

        _bgmFade = target;
        ApplyVolumes();
    }

    // ─────────────────────────────────────────────────────
    // Setup
    // ─────────────────────────────────────────────────────

    private void CreateSources()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.spatialBlend = 0f;
        _bgmSource.priority = 0;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;

        _bgmFade = 0f;
    }

    private void LoadSfxClips()
    {
        foreach (SfxType type in System.Enum.GetValues(typeof(SfxType)))
        {
            var clip = Resources.Load<AudioClip>(SfxPath + type);
            if (clip != null)
                _sfxClips[type] = clip;
            else
                Debug.LogWarning($"[SoundManager] SFX 클립 없음: Resources/{SfxPath}{type}");
        }
    }

    private void ApplyVolumes()
    {
        if (_bgmSource != null) _bgmSource.volume = bgmVolume * bgmMaxVolume * _bgmFade;
        if (_sfxSource != null) _sfxSource.volume = sfxVolume;
    }
}
