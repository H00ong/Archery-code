using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Inventory 탭의 컨테이너 View.
    /// 3개의 sub-tab 버튼(Weapon/Armor/Shoes)이 하나의 공유 UI_EquipmentTabView를 갱신하는 방식이다.
    /// (Character 탭과 동일하게 좌/우 버튼 + 이미지 1세트만 존재하고, sub-tab은 표시 중인 EquipmentType만 바꾼다.)
    /// 인덱스 매핑: 0 = Weapon, 1 = Armor, 2 = Shoes (기본 탭은 0번 = Weapon).
    /// </summary>
    public class UI_InventoryTabView : MonoBehaviour
    {
        public const int SubTabWeapon = 0;
        public const int SubTabArmor = 1;
        public const int SubTabShoes = 2;

        [Header("Sub Tabs (0:Weapon, 1:Armor, 2:Shoes)")]
        [SerializeField] private Button[] subTabButtons;

        [Header("공유 장비 View")]
        [SerializeField] private UI_EquipmentTabView equipmentTabView;

        public UI_EquipmentTabView GetEquipmentTabView() => equipmentTabView;

        public int SubTabCount => subTabButtons != null ? subTabButtons.Length : 0;

        public void Init(Action<int> onSubTabSelected)
        {
            if (subTabButtons == null) return;

            for (int i = 0; i < subTabButtons.Length; i++)
            {
                int index = i;
                subTabButtons[i].onClick.RemoveAllListeners();
                subTabButtons[i].onClick.AddListener(() => onSubTabSelected?.Invoke(index));
            }
        }
    }
}
