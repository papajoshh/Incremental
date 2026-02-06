namespace Runtime.Infraestructure.MoñecoStates
{
    public class TurningState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) => null;

        public void OnComplete(MoñecoMonoBehaviour m)
        {
            m.Direction *= -1;
            m.ChangeState<WalkingState>();
        }
    }
}
