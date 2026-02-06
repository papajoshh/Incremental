using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class FallingState : IMoñecoState
    {
        public int? GetAnimationHash(MoñecoMonoBehaviour m) => AnimHashes.Falling;

        public void OnStep(MoñecoMonoBehaviour m)
        {
            if (m.CheckGroundBelow(out var hit))
            {
                m.Move(Vector3.down * hit.distance);
                m.ChangeState<LandingState>();
                return;
            }

            m.Move(Vector3.down * m.FallStepDistance);
        }
    }
}
