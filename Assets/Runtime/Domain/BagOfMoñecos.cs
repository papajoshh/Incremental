using System;

namespace Runtime
{
    public class BagOfMoñecos
    {
        public int MoñecosInside { get; private set; }

        public event Action<int> OnMoñecosChange;
        
        public void Add()
        {
            MoñecosInside++;
            OnMoñecosChange?.Invoke(MoñecosInside);
        }
        public void Remove(int _howMany)
        {
            MoñecosInside -= _howMany;
            if (MoñecosInside < 0)
                MoñecosInside = 0;
            OnMoñecosChange?.Invoke(MoñecosInside);
        }
    }
}