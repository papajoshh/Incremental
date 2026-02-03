namespace Runtime.Infraestructure.MoñecoStates
{
    public class LandingState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => AnimHashes.Landing;

        public void OnComplete(IMoñecoContext ctx) => ctx.ChangeState<WalkingState>();
    }
}
