using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class FallingState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) => AnimHashes.Falling;

        public void OnStep(IMoñecoContext ctx)
        {
            if (ctx.CheckGroundBelow(out var hit))
            {
                ctx.Move(Vector3.down * hit.distance);
                ctx.ChangeState<LandingState>();
                return;
            }

            ctx.Move(Vector3.down * ctx.FallStepDistance);
        }
    }
}
