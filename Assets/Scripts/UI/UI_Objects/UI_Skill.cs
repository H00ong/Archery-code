using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class UI_Skill : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI _skillName;
    [SerializeField] TextMeshProUGUI _skillLevel;
    [SerializeField] TextMeshProUGUI _skillDescription;

    public event Action Clicked;


    // --- 표시 전용 API ---
    public void SetName(string s) => _skillName.text = s;
    public void SetLevel(int cur, int max) => _skillLevel.text = $"Lv.{cur}/{max}";
    public void SetDescription(string s) => _skillDescription.text = s;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        Clicked?.Invoke();
    }
}