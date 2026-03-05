using Players;
using UI;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// 씨에 존재하는 모든 UI View 참조를 보관하고,
    /// 게임 로직에서 팔업 표시 요청을 받아 Presenter에 위임한다.
    /// UI는 옴에 파쇼되므로 DontDestroyOnLoad 미사용.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Skill")]
        [SerializeField] private GameObject skillChoicePopupPrefab;

        private SkillChoicePopupPresenter _skillChoicePresenter;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// 레벨업 시 스킬 선택 팬업을 표시한다.
        /// LevelManager가 호용.
        /// </summary>
        public void ShowSkillChoicePopup(PlayerSkill playerSkill)
        {
            if (_skillChoicePresenter == null)
            {
                var canvas  = FindFirstObjectByType<Canvas>();
                var popupObj = Instantiate(skillChoicePopupPrefab, canvas.transform);
                var popupView = popupObj.GetComponent<SkillChoicePopupView>();
                _skillChoicePresenter = new SkillChoicePopupPresenter(popupView);
            }

            _skillChoicePresenter.Show(playerSkill);
        }

        public void ClearDataInStage()
        {
            _skillChoicePresenter = null;
        }
    }
}
