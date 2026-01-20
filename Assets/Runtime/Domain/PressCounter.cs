using System;

namespace Runtime
{
    public class PressCounter
    {
        public int Presses { get; private set; }
        public int AddPerPress { get; private set; } = 1;
        public event Action<int> OnSpacePress;

        public static PressCounter StartWith(int presses, int addPerPress)
        {
            return new PressCounter()
            {
                Presses = presses,
                AddPerPress = addPerPress
            };
        }
        public void Press()
        {
            Presses += AddPerPress;
            OnSpacePress?.Invoke(Presses);
        }
    }
}