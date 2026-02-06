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
        [Inject] private readonly DiContainer _diContainer;

        public MoñecoInstantiator(GameObject prefab)
        {
            _moñecoPrefab = prefab;
        }
        public async Task<MoñecoMonoBehaviour> GiveBirth(Vector3 positionToSpawn)
        {
            var moñeco = _diContainer.InstantiatePrefabForComponent<MoñecoMonoBehaviour>(_moñecoPrefab, positionToSpawn, Quaternion.identity, null);
            _saveHandler.Track(moñeco);
            await moñeco.Birth();
            _bagOfMoñecos.Add();
            return moñeco;
        }

        public MoñecoMonoBehaviour Spawn(Vector3 position)
        {
            var moñeco = _diContainer.InstantiatePrefabForComponent<MoñecoMonoBehaviour>(_moñecoPrefab, position, Quaternion.identity, null);
            _saveHandler.Track(moñeco);
            return moñeco;
        }
    }
}