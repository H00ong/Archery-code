using System.Collections.Generic;
using Managers;
using Players;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 HUD.
/// - 체력 슬라이더 + 현재 장착 캐릭터 이미지
/// - 경험치 슬라이더 + 슬라이더 내부 레벨 텍스트
/// - toggleKey(기본 Tab)를 누르는 동안만 획득 스킬 아이콘 목록 표시 (hold 방식)
/// 싱글톤(PlayerController / LevelManager / CharacterManager / SkillManager)을 참조하여
/// 매 프레임 폴링으로 갱신한다. (UI_ProgressBar와 동일한 폴링 패턴)
/// </summary>
public class InGameHud : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;        // 선택: "현재/최대" 표시
    [SerializeField] private Image characterIcon;           // 현재 장착 캐릭터 이미지

    [Header("Experience / Level")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;     // 경험치 슬라이더 안에 배치되는 레벨 텍스트

    [Header("Skill List (Tab)")]
    [SerializeField] private GameObject skillListPanel;     // Tab으로 켜고 끄는 패널 루트 (아이콘 컨테이너 겸용 가능)
    [SerializeField] private Transform skillIconContainer;  // 아이콘 생성 부모. 비워두면 skillListPanel 자체를 사용
    [SerializeField] private Image skillIconPrefab;         // 스킬 아이콘 1개 프리팹 (Image)
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private Health _health;
    // 스킬 ID → 아이콘 Image. 한 번 만든 아이콘은 재사용하고, 새 스킬만 추가 생성한다.
    private readonly Dictionary<string, Image> _iconDict = new();

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.PlayerSpawned, OnPlayerSpawned);
        EventBus.Subscribe(EventType.SkillChosen, OnSkillChosen);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.PlayerSpawned, OnPlayerSpawned);
        EventBus.Unsubscribe(EventType.SkillChosen, OnSkillChosen);
    }

    private void Start()
    {
        if (skillListPanel != null)
            skillListPanel.SetActive(false);

        // 이미 플레이어가 스폰된 상태에서 HUD가 켜질 수도 있으므로 한 번 초기화한다.
        if (PlayerController.Instance != null)
            OnPlayerSpawned();
    }

    private void Update()
    {
        UpdateHealth();
        UpdateExp();
        HandleSkillToggle();
    }

    private void OnPlayerSpawned()
    {
        var pc = PlayerController.Instance;
        if (pc == null) return;

        _health = pc.Health;

        var identity = CharacterManager.Instance != null
            ? CharacterManager.Instance.GetCurrentCharacterIdentity()
            : null;

        if (characterIcon != null && identity != null && identity.characterIcon != null)
        {
            characterIcon.sprite = identity.characterIcon;
            characterIcon.enabled = true;
        }

        // 새 판이 시작되면 이전 판의 스킬 아이콘을 모두 제거한다.
        ClearSkillIcons();
    }

    private void UpdateHealth()
    {
        if (_health == null) return;

        int max = _health.MaxHealth;
        int cur = _health.CurrentHealth;

        if (hpSlider != null)
            hpSlider.value = max > 0 ? (float)cur / max : 0f;

        if (hpText != null)
            hpText.text = $"{cur}/{max}";
    }

    private void UpdateExp()
    {
        var lm = LevelManager.Instance;
        if (lm == null) return;

        if (levelText != null)
            levelText.text = $"Lv.{lm.currentLevel}";

        if (expSlider == null) return;

        if (lm.currentLevel >= lm.maxLevel)
        {
            expSlider.value = 1f;
            return;
        }

        int required = lm.RequiredExp;
        expSlider.value = required > 0 ? (float)lm.currentExp / required : 0f;
    }

    private void HandleSkillToggle()
    {
        if (skillListPanel == null) return;

        bool held    = Input.GetKey(toggleKey);
        bool visible = skillListPanel.activeSelf;

        if (held && !visible)
        {
            // 키를 막 누른 순간 — 패널을 보이고 새 스킬만 추가
            skillListPanel.SetActive(true);
            RefreshSkillIcons();
        }
        else if (!held && visible)
        {
            // 키를 뗀 순간 — 패널만 숨긴다. 아이콘은 파괴하지 않고 재사용한다.
            skillListPanel.SetActive(false);
        }
        else if (held && visible)
        {
            // 이미 열려 있는 동안에도 매 프레임 체크해 새로 배운 스킬 아이콘을 즉시 추가한다.
            RefreshSkillIcons();
        }
    }

    private void OnSkillChosen()
    {
        // 스킬을 새로 획득/강화했고 목록이 열려 있으면 즉시 갱신한다.
        if (skillListPanel != null && skillListPanel.activeSelf)
            RefreshSkillIcons();
    }

    private void RefreshSkillIcons()
    {
        var container = skillIconContainer != null ? skillIconContainer : skillListPanel?.transform;
        if (container == null) return;

        if (skillIconPrefab == null)
        {
            Debug.LogWarning("[InGameHud] skillIconPrefab이 할당되지 않았습니다. 인스펙터에서 스킬 아이콘 Image 프리팹을 연결하세요.");
            return;
        }

        var pc = PlayerController.Instance;
        if (pc == null || pc.Skill == null) return;

        var skillManager = SkillManager.Instance;
        if (skillManager == null) return;

        foreach (var id in pc.Skill.acquiredSkillModule.Keys)
        {
            // 이미 아이콘이 있는 스킬은 건너뛴다 (새로 배운 것만 생성).
            if (_iconDict.ContainsKey(id)) continue;

            if (!skillManager.SkillDict.TryGetValue(id, out var def) || def == null)
            {
                Debug.LogWarning($"[InGameHud] 스킬 ID '{id}'를 SkillDict에서 찾을 수 없습니다.");
                continue;
            }

            var icon = Instantiate(skillIconPrefab, container);

            if (def.icon != null)
                icon.sprite = def.icon;
            else
                Debug.LogWarning($"[InGameHud] '{id}' SkillDefinition.icon이 null입니다. 인스펙터에서 Sprite를 할당하세요.");

            icon.enabled = true;
            _iconDict[id] = icon;
        }
    }

    private void ClearSkillIcons()
    {
        foreach (var icon in _iconDict.Values)
            if (icon != null) Destroy(icon.gameObject);
        _iconDict.Clear();
    }
}
