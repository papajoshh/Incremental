namespace Runtime.Infraestructure.MoñecoStates
{
    public class TurningState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => null;

        public void OnComplete(IMoñecoContext ctx)
        {
            ctx.Direction *= -1;
            ctx.ChangeState<WalkingState>();
        }
    }
}
