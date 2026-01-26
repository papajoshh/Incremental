using UnityEngine;

namespace Runtime.Application
{
    public interface Interactor
    {
        void OnInteractionTick();
        void StopInteraction();
        void SetPositionToInteract(Transform transform);
    }
}