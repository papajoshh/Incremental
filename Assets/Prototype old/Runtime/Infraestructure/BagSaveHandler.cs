using Runtime.Application;
using Runtime.Domain;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class BagSaveHandler : MonoBehaviour, ISaveable
    {
        [Inject] private BagOfMoñecos _bag;

        public string SaveId => "bag";
        public int RestoreOrder => 0;

        public string CaptureStateJson() => JsonUtility.ToJson(_bag.CaptureState());

        public void RestoreStateJson(string json) => _bag.Load(JsonUtility.FromJson<BagSaveData>(json));
    }
}
