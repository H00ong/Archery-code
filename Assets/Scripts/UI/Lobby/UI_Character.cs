using Managers;
using Players;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Character : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CharacterIdentity characterIdentity;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlayerManager.Instance.SetCurrentCharacter(characterIdentity);
        }
    }
}
