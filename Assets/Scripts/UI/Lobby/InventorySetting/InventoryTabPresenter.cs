using Players;

namespace UI
{
    /// <summary>
    /// Inventory 탭(상위)의 Presenter.
    /// 3개의 EquipmentTabPresenter(Weapon/Armor/Shoes)를 보유하고 sub-tab 전환을 관리한다.
    /// 기본 sub-tab은 Weapon(인덱스 0)이다.
    /// </summary>
    public class InventoryTabPresenter
    {
        private readonly UI_InventoryTabView _view;

        private readonly EquipmentTabPresenter _weaponPresenter;
        private readonly EquipmentTabPresenter _armorPresenter;
        private readonly EquipmentTabPresenter _shoesPresenter;

        private int _currentSubTab;

        public InventoryTabPresenter(UI_InventoryTabView view)
        {
            _view = view;

            var weaponView = _view.GetEquipmentTabView(UI_InventoryTabView.SubTabWeapon);
            var armorView  = _view.GetEquipmentTabView(UI_InventoryTabView.SubTabArmor);
            var shoesView  = _view.GetEquipmentTabView(UI_InventoryTabView.SubTabShoes);

            _weaponPresenter = new EquipmentTabPresenter(weaponView, EquipmentType.Weapon);
            _armorPresenter  = new EquipmentTabPresenter(armorView,  EquipmentType.Armor);
            _shoesPresenter  = new EquipmentTabPresenter(shoesView,  EquipmentType.Shoes);

            _view.Init(OnSubTabSelected);
        }

        public void Activate()
        {
            // 진입 시 항상 기본 sub-tab(Weapon)으로 초기화
            _currentSubTab = UI_InventoryTabView.SubTabWeapon;
            _view.SwitchSubTab(_currentSubTab);
            GetPresenter(_currentSubTab).Activate();
        }

        public void Deactivate()
        {
            // 활성화돼 있던 sub-presenter만 정리
            GetPresenter(_currentSubTab).Deactivate();
        }

        private void OnSubTabSelected(int index)
        {
            if (index == _currentSubTab) return;

            GetPresenter(_currentSubTab).Deactivate();

            _currentSubTab = index;
            _view.SwitchSubTab(_currentSubTab);

            GetPresenter(_currentSubTab).Activate();
        }

        private EquipmentTabPresenter GetPresenter(int subTabIndex) => subTabIndex switch
        {
            UI_InventoryTabView.SubTabWeapon => _weaponPresenter,
            UI_InventoryTabView.SubTabArmor  => _armorPresenter,
            UI_InventoryTabView.SubTabShoes  => _shoesPresenter,
            _ => _weaponPresenter,
        };
    }
}
