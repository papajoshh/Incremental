using UnityEngine;
using Zenject;

namespace Programental
{
    public abstract class GoldenCodeMilestoneReward : MonoBehaviour
    {
        public abstract string RewardId { get; }
        public bool Unlocked { get; private set; }

        [Inject]
        private void RegisterInTracker(GoldenCodeMilestoneTracker tracker)
        {
            tracker.Register(RewardId, this);
        }

        public abstract void OnUnlock();

        public virtual void Restore()
        {
            Unlocked = true;
        }

        public void Unlock()
        {
            if (Unlocked) return;
            Unlocked = true;
            OnUnlock();
        }
    }
}
