using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class Skill_View : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI _skillName;
    [SerializeField] TextMeshProUGUI _skillLevel;
    [SerializeField] TextMeshProUGUI _skillDescription;

    private Action _onClicked;

    // --- 표시 전용 API ---
    public void SetName(string s) => _skillName.text = s;
    public void SetLevel(int cur, int max) => _skillLevel.text = $"Lv.{cur}/{max}";
    public void SetDescription(string s) => _skillDescription.text = s;
    public void SetClickedAction(Action clickedAction) => _onClicked = clickedAction;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _onClicked?.Invoke();
    }
}