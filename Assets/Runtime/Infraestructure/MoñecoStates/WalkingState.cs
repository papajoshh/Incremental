using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public class WalkingState : IMoñecoState
    {
        public int? GetAnimationHash(IMoñecoContext ctx) =>
            ctx.Direction > 0 ? AnimHashes.WalkRight : AnimHashes.WalkLeft;

        public void OnStep(IMoñecoContext ctx)
        {
            if (ctx.CheckWallAhead(out _))
            {
                ctx.ChangeState<TurningState>();
                return;
            }

            ctx.Move(Vector3.right * ctx.Direction * ctx.StepDistance);

            if (ctx.HasReachedInteractionTarget())
                ctx.ArriveAtInteraction();
        }

        public void OnUpdate(IMoñecoContext ctx)
        {
            if (!ctx.IsGrounded())
                ctx.ChangeState<FallingState>();
        }
    }
}
