namespace Runtime.Infraestructure.MoñecoStates
{
    public class IdleState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => AnimHashes.Idle;

        public void OnUpdate(IMoñecoContext ctx)
        {
            if (!ctx.IsGrounded())
                ctx.ChangeState<FallingState>();
        }
    }
}
