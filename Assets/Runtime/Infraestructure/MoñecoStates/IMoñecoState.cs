namespace Runtime.Infraestructure.MoñecoStates
{
    public interface IMoñecoState
    {
        int? GetAnimationHash(IMoñecoContext ctx);
        void OnEnter(IMoñecoContext ctx) { }
        void OnStep(IMoñecoContext ctx) { }
        void OnComplete(IMoñecoContext ctx) { }
        void OnUpdate(IMoñecoContext ctx) { }
    }
}
