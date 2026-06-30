using Managers;
using UnityEngine;

namespace Objects
{
    public class ExpItem : CollectItem
    {
        [SerializeField] private int expAmount = 10;

        public void SetAmount(int amount) => expAmount = amount;

        protected override void OnCollected()
        {
            LevelManager.Instance.AddExp(expAmount);
        }
    }
}
