using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public interface IMoñecoContext
    {
        Transform Transform { get; }
        int Direction { get; set; }
        float StepDistance { get; }
        float FallStepDistance { get; }

        void ChangeState<T>() where T : IMoñecoState, new();
        void EvaluateAir();

        bool IsGrounded();
        bool CheckWallAhead(out RaycastHit2D hit);
        bool CheckGroundBelow(out RaycastHit2D hit);
        bool HasReachedInteractionTarget();

        void Move(Vector3 delta);
        void ArriveAtInteraction();
        void TickInteraction();
        void CompleteBirth();
        void DestroySelf();
        int GetInteractionAnimationHash();
    }
}
