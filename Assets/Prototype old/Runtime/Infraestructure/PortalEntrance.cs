using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class PortalEntrance: MonoBehaviour
    {
        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;
        [Inject] private BagOfMoñecosCanvas bagCanvas;
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.transform.parent.TryGetComponent<MoñecoMonoBehaviour>(out var moñeco))
            {
                _ = EnterPortal(moñeco);
            }
        }
        
        private async Task EnterPortal(MoñecoMonoBehaviour moñeco)
        {
            await moñeco.EnterPortal();
            _bagOfMoñecos.PutInside();
            bagCanvas.Enable();
        }
    }
}