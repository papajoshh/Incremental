namespace Runtime.Infraestructure.MoñecoStates
{
    public class IdleState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) => AnimHashes.Idle;

        public void OnUpdate(MoñecoMonoBehaviour m)
        {
            if (!m.IsGrounded())
                m.ChangeState<FallingState>();
        }
    }
}
