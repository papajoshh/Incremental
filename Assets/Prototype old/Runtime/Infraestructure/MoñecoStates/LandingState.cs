namespace Runtime.Infraestructure.MoñecoStates
{
    public class LandingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) => AnimHashes.Landing;

        public void OnComplete(MoñecoMonoBehaviour m) => m.ChangeState<WalkingState>();
    }
}
