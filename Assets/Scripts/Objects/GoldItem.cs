using Managers;
using UnityEngine;

namespace Objects
{
    public class GoldItem : CollectItem
    {
        [SerializeField] private int goldAmount = 10;

        public void SetAmount(int amount) => goldAmount = amount;

        protected override void OnCollected()
        {
            PlayerManager.Instance.EarnGold(goldAmount);
        }
    }
}
