using System;

namespace Runtime
{
    public class PressWitCap
    {
        private PressCounter _pressCounter;
        private int _cap;
        
        public bool Completed => _pressCounter.Presses >= _cap;
        public float Percentage => Math.Min(1f, (float)_pressCounter.Presses / _cap);
        public static PressWitCap StartWith(int initialPresses, int pressesPerImpulse, int cap)
        {
            return new PressWitCap()
            {
                _pressCounter = PressCounter.StartWith(initialPresses, pressesPerImpulse),
                _cap = cap
            };
        }
        
        public void Press()
        {
            _pressCounter.Press();
        }
        
        public void Reset()
        {
            _pressCounter.Reset();
        }
    }
}