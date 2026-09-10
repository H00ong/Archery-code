using Managers;

namespace Objects
{
    public class GoldItem : CollectItem
    {
        private int goldAmount;

        public void SetAmount(int amount) => goldAmount = amount;

        protected override void OnCollected()
        {
            PlayerManager.Instance.EarnGold(goldAmount);
        }
    }
}
