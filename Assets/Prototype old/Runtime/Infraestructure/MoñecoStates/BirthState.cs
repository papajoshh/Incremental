namespace Runtime.Infraestructure.MoñecoStates
{
    public class BirthState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) => AnimHashes.Birth;

        public void OnComplete(MoñecoMonoBehaviour m)
        {
            m.CompleteBirth();
            m.EvaluateAir();
        }
    }
}
