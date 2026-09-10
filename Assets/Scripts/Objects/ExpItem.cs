using Managers;

namespace Objects
{
    public class ExpItem : CollectItem
    {
        private int expAmount;

        public void SetAmount(int amount) => expAmount = amount;

        protected override void OnCollected()
        {
            LevelManager.Instance.AddExp(expAmount);
        }
    }
}
