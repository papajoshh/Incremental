using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class MoñecoInstantiator
    {
        private readonly GameObject _moñecoPrefab;

        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;
        [Inject] private readonly MoñecosSaveHandler _saveHandler;

        public MoñecoInstantiator(GameObject prefab)
        {
            _moñecoPrefab = prefab;
        }
        public async Task<MoñecoMonoBehaviour> GiveBirth(Vector3 positionToSpawn)
        {
            var moñeco = GameObject.Instantiate(_moñecoPrefab, positionToSpawn, Quaternion.identity).GetComponent<MoñecoMonoBehaviour>();
            _saveHandler.Track(moñeco);
            await moñeco.Birth();
            _bagOfMoñecos.Add();
            return moñeco;
        }

        public MoñecoMonoBehaviour Spawn(Vector3 position)
        {
            var moñeco = GameObject.Instantiate(_moñecoPrefab, position, Quaternion.identity).GetComponent<MoñecoMonoBehaviour>();
            _saveHandler.Track(moñeco);
            return moñeco;
        }
    }
}