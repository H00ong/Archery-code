using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Map : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int mapIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            MapManager.Instance.SetCurrentMapIndex(mapIndex);
        }
    }
}
