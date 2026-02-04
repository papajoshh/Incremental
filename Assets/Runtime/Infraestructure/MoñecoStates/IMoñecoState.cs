namespace Runtime.Infraestructure.MoñecoStates
{
    public interface IMoñecoState
    {
        int? GetAnimationHash(MoñecoMonoBehaviour m);
        void OnEnter(MoñecoMonoBehaviour m) { }
        void OnStep(MoñecoMonoBehaviour m) { }
        void OnComplete(MoñecoMonoBehaviour m) { }
        void OnUpdate(MoñecoMonoBehaviour m) { }
    }
}
