using System.Threading.Tasks;

namespace Runtime.Application
{
    public interface Interactable
    {
        bool CanInteract(Interactor interactor);
        void StartInteraction(Interactor interactor);
        Task OnInteractionTick(Interactor interactor);
        void EndInteraction(Interactor interactor);
    }
}