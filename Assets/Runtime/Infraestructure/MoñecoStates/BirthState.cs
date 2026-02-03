namespace Runtime.Infraestructure.MoñecoStates
{
    public class BirthState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => AnimHashes.Birth;

        public void OnComplete(IMoñecoContext ctx)
        {
            ctx.CompleteBirth();
            ctx.EvaluateAir();
        }
    }
}
