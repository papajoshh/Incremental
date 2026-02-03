namespace Runtime.Infraestructure.MoñecoStates
{
    public class GoToBagState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => AnimHashes.GoToBag;

        public void OnComplete(IMoñecoContext ctx) => ctx.DestroySelf();
    }
}
