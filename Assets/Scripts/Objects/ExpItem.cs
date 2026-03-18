using Managers;
using UnityEngine;

namespace Objects
{
    public class ExpItem : CollectItem
    {
        [SerializeField] private int expAmount = 10;

        protected override void OnCollected()
        {
            LevelManager.Instance.AddExp(expAmount);
        }
    }
}
