using System;

namespace Runtime
{
    public class Keyboard
    {
        public int SpacePresses { get; private set; }
        public event Action<int> OnSpacePress;
        
        public void Press()
        {
            SpacePresses++;
            OnSpacePress?.Invoke(SpacePresses);
        }
    }
}
