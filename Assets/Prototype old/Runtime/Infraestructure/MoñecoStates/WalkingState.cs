using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class WalkingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) =>
            m.Direction > 0 ? AnimHashes.WalkRight : AnimHashes.WalkLeft;

        public void OnStep(MoñecoMonoBehaviour m)
        {
            if (m.CheckWallAhead(out _))
            {
                m.ChangeState<TurningState>();
                return;
            }

            m.Move(Vector3.right * m.Direction * m.StepDistance);

            if (m.HasReachedInteractionTarget())
                m.ArriveAtInteraction();
        }

        public void OnUpdate(MoñecoMonoBehaviour m)
        {
            if (!m.IsGrounded())
                m.ChangeState<FallingState>();
        }
    }
}
