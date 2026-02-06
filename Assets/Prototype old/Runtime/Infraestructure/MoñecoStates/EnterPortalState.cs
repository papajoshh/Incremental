namespace Runtime.Infraestructure.MoñecoStates
{
    public class EnterPortalState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) => AnimHashes.EnterPortal;

        public void OnComplete(MoñecoMonoBehaviour m) => m.DestroySelf();
    }
}
