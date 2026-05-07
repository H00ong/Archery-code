using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Inventory 탭의 컨테이너 View.
    /// 3개의 sub-tab 버튼(Weapon/Armor/Shoes)과 3개의 sub-content GameObject를 갖는다.
    /// 각 sub-content에는 UI_EquipmentTabView가 부착되어 있어야 한다.
    /// 인덱스 매핑: 0 = Weapon, 1 = Armor, 2 = Shoes (기본 탭은 0번 = Weapon).
    /// </summary>
    public class UI_InventoryTabView : MonoBehaviour
    {
        public const int SubTabWeapon = 0;
        public const int SubTabArmor = 1;
        public const int SubTabShoes = 2;

        [Header("Sub Tabs (0:Weapon, 1:Armor, 2:Shoes)")]
        [SerializeField] private Button[] subTabButtons;
        [SerializeField] private GameObject[] subTabContents;

        public UI_EquipmentTabView GetEquipmentTabView(int subTabIndex)
        {
            if (subTabContents == null || subTabIndex < 0 || subTabIndex >= subTabContents.Length)
                return null;
            return subTabContents[subTabIndex].GetComponent<UI_EquipmentTabView>();
        }

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

        public void SwitchSubTab(int activeIndex)
        {
            if (subTabContents == null) return;
            for (int i = 0; i < subTabContents.Length; i++)
                subTabContents[i].SetActive(i == activeIndex);
        }
    }
}
