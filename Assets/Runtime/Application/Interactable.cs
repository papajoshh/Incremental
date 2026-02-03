using System.Threading.Tasks;
using Runtime.Domain;

namespace Runtime.Application
{
    public interface Interactable
    {
        InteractionInfo CurrentInteractionInfo { get;  }
        bool CanInteract(Interactor interactor);
        void StartInteraction(Interactor interactor);
        Task OnInteractionTick(Interactor interactor);
        void EndInteraction(Interactor interactor);
    }
}