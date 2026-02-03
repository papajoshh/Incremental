using UnityEngine;

namespace Runtime.Application
{
    public interface Interactor
    {
        void PauseInteraction();
        void ResumeInteraction();
        void StopInteraction();
        void SetPositionToInteract(Transform transform);
    }
}