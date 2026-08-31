using Players;

namespace UI
{
    /// <summary>
    /// Inventory 탭(상위)의 Presenter.
    /// 하나의 EquipmentTabPresenter(공유 View)를 보유하고, sub-tab 전환 시 대상 EquipmentType만 바꿔서 갱신한다.
    /// 기본 sub-tab은 Weapon(인덱스 0)이다.
    /// </summary>
    public class InventoryTabPresenter
    {
        private static readonly EquipmentType[] SubTabTypes =
        {
            EquipmentType.Weapon, EquipmentType.Armor, EquipmentType.Shoes,
        };

        private readonly UI_InventoryTabView _view;
        private readonly EquipmentTabPresenter _presenter;

        private int _currentSubTab;

        public InventoryTabPresenter(UI_InventoryTabView view)
        {
            _view = view;

            _presenter = new EquipmentTabPresenter(_view.GetEquipmentTabView());

            _view.Init(OnSubTabSelected);
        }

        public void Activate()
        {
            // 진입 시 항상 기본 sub-tab(Weapon)으로 초기화
            _currentSubTab = UI_InventoryTabView.SubTabWeapon;
            _presenter.Activate(SubTabTypes[_currentSubTab]);
        }

        public void Deactivate()
        {
            _presenter.Deactivate();
        }

        private void OnSubTabSelected(int index)
        {
            if (index == _currentSubTab) return;

            _presenter.Deactivate();

            _currentSubTab = index;
            _presenter.Activate(SubTabTypes[_currentSubTab]);
        }
    }
}
