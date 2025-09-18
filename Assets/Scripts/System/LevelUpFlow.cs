using UnityEngine;

public class LevelUpFlow : MonoBehaviour
{
    [SerializeField] SkillChoicePopupView _skillChoicePopup;
    [SerializeField] Transform _popupContainer;
    [SerializeField] PlayerSkill _playerSkill; // 씬의 기존 컴포넌트 참조

    public static event System.Action OnLevelUp; // 레벨업 시그널
    public static event System.Action OnSkillChosen; // 스킬 선택 시그널

    private void Start()
    {
        if (_playerSkill == null)
            _playerSkill = FindAnyObjectByType<PlayerSkill>();

        OnLevelUp += ShowSkillChoicePopup;
    }

    public void ShowSkillChoicePopup()
    {
        var popup = Instantiate(_skillChoicePopup, _popupContainer);
        var presenter = new SkillChoicePresenter(
            skillPopup : popup, _playerSkill, onCompleted: OnSkillChosen);

        presenter.Show();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            OnLevelUp?.Invoke();
    }
}