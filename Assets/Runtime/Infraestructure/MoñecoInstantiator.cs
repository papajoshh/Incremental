using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class MoñecoInstantiator
    {
        private readonly GameObject _moñecoPrefab;
        
        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;

        public MoñecoInstantiator(GameObject prefab)
        {
            _moñecoPrefab = prefab;
        }
        public async Task<MoñecoMonoBehaviour> GiveBirth(Vector3 positionToSpawn)
        {
            var moñeco = GameObject.Instantiate(_moñecoPrefab, positionToSpawn, Quaternion.identity).GetComponent<MoñecoMonoBehaviour>();
            await moñeco.Birth();
            _bagOfMoñecos.Add();
            return moñeco;
        }
    }
}