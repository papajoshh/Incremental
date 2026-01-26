using System;

namespace Runtime
{
    public class PartOfBody
    {
        private readonly PressCounter _press;
        private readonly int _pressToComplete;
        private readonly int _addPerPress = 1;
        public Action<int> OnAdd;
        public Action OnComplete;
        public float PercentagePerClick => _addPerPress / _pressToComplete;
        public float PercentageFullfilled => (float)_press.Presses / _pressToComplete;
        public bool Fullfilled => _press.Presses >= _pressToComplete;
    
        public PartOfBody(int presses)
        {
            _press = PressCounter.StartWith(0, _addPerPress);
            _pressToComplete = presses;
        }

        public static PartOfBody Head()
        {
            return new PartOfBody(1);
        }
        public static PartOfBody Body()
        {
            return new PartOfBody(1);
        }
        public static PartOfBody LeftArm()
        {
            return new PartOfBody(1);
        }
        public static PartOfBody RightArm()
        {
            return new PartOfBody(1);
        }
        public static PartOfBody RightLeg()
        {
            return new PartOfBody(1);
        }
        public static PartOfBody LeftLeg()
        {
            return new PartOfBody(1);
        }

        public void Press()
        {
            if (Fullfilled) return;
            _press.Press();
            OnAdd?.Invoke(_press.Presses);
            if(Fullfilled) OnComplete?.Invoke();
        }
    }
}