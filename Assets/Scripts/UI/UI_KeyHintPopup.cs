using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary> 특정 키를 눌러 다음으로 진행하라는 안내 팝업. 키 이름은 GameKeys 값으로 채워진다. </summary>
    public class UI_KeyHintPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hintText;
        [Tooltip("{0}이 키 이름으로 치환된다.")]
        [SerializeField] private string format = "Press [{0}] to continue";

        public void Show(KeyCode key)
        {
            if (hintText) hintText.text = string.Format(format, GameKeys.GetDisplayName(key));
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
