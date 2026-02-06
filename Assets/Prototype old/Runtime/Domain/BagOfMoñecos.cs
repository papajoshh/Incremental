using System;
using Runtime.Domain;

namespace Runtime
{
    public class BagOfMoñecos
    {
        public int MoñecosInside { get; private set; }
        public int MoñecosOutside => TotalMoñecos - MoñecosInside;
        public int TotalMoñecos { get; private set; }

        public event Action<int> OnMoñecosChange;
        public event Action<int> OnMoñecosInsideChange;

        public void Add()
        {
            TotalMoñecos++;
            OnMoñecosChange?.Invoke(TotalMoñecos);
        }

        public void PutInside()
        {
            MoñecosInside++;
            OnMoñecosInsideChange?.Invoke(MoñecosInside);
        }

        public BagSaveData CaptureState()
        {
            return new BagSaveData
            {
                total = TotalMoñecos,
                inside = MoñecosInside
            };
        }

        public void Load(BagSaveData data)
        {
            TotalMoñecos = data.total;
            MoñecosInside = data.inside;
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
