namespace Runtime.Infraestructure.MoñecoStates
{
    public class InteractingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m)
        {
            var interactable = m.CurrentInteractable;
            if (interactable == null) return AnimHashes.Interacting;

            return interactable.CurrentInteractionInfo.InteractionAnimation switch
            {
                "RepairComputer" => AnimHashes.InteractingMachine,
                _ => AnimHashes.Interacting
            };
        }

        public void OnStep(MoñecoMonoBehaviour m)
        {
            m.CurrentInteractable?.OnInteractionTick(m);
        }
    }
}
