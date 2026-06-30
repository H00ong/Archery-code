using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 전형적인 게임 확인 팝업. 메시지와 확인(체크)/취소(X) 버튼으로 구성된다.
/// Show(message, onConfirm, onCancel) 으로 열고, 버튼을 누르면 자동으로 닫힌다.
/// ESC 등 외부 입력으로 닫고 싶을 때는 Cancel() 을 호출하면 된다.
/// </summary>
public class ConfirmPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton; // 체크 버튼
    [SerializeField] private Button cancelButton;   // X 버튼

    private UnityAction _onConfirm;
    private UnityAction _onCancel;

    /// <summary>현재 팝업이 열려 있는지 여부.</summary>
    public bool IsOpen => gameObject.activeSelf;

    /// <summary>
    /// 확인 팝업을 띄운다.
    /// </summary>
    /// <param name="message">표시할 안내 문구.</param>
    /// <param name="onConfirm">체크 버튼을 눌렀을 때 실행할 동작.</param>
    /// <param name="onCancel">X 버튼(또는 ESC)을 눌렀을 때 실행할 동작(선택).</param>
    public void Show(string message, UnityAction onConfirm, UnityAction onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (messageText != null)
            messageText.text = message;

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(Confirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Cancel);
        }

        gameObject.SetActive(true);
    }

    /// <summary>확인(체크) 동작. 버튼 클릭으로 호출된다.</summary>
    public void Confirm()
    {
        var callback = _onConfirm;
        Hide();
        callback?.Invoke();
    }

    /// <summary>취소(X) 동작. 버튼 클릭이나 ESC 등 외부에서 호출할 수 있다.</summary>
    public void Cancel()
    {
        var callback = _onCancel;
        Hide();
        callback?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
