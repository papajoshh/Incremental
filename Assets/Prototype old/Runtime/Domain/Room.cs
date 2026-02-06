using System;

namespace Runtime.Domain
{
    public class Room
    {
        public bool Discovered { get; private set; }
        public Action OnDiscovered;

        public void RestoreDiscovered()
        {
            Discovered = true;
        }

        public void Discover()
        {
            if (Discovered) return;
            Discovered = true;
            OnDiscovered?.Invoke();
        }
    }
}
