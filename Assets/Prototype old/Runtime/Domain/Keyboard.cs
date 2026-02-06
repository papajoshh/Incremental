using System;

namespace Runtime
{
    public class Keyboard
    {
        public int SpacePresses { get; private set; }
        public int AddPerPress { get; private set; } = 1;
        public event Action<int> OnSpacePress;
        
        public void Press()
        {
            SpacePresses += AddPerPress;
            OnSpacePress?.Invoke(SpacePresses);
        }
    }
}
