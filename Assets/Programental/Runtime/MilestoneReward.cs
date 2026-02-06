using UnityEngine;

namespace Programental
{
    public abstract class MilestoneReward : MonoBehaviour
    {
        public bool Unlocked { get; private set; }

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
