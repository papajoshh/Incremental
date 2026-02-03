namespace Runtime.Infraestructure.MoñecoStates
{
    public class InteractingState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => ctx.GetInteractionAnimationHash();

        public void OnStep(IMoñecoContext ctx) => ctx.TickInteraction();
    }
}
