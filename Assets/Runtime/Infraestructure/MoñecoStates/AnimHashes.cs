using UnityEngine;

namespace Runtime.Infraestructure.MoñecoStates
{
    public static class AnimHashes
    {
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int WalkRight = Animator.StringToHash("WalkRight");
        public static readonly int WalkLeft = Animator.StringToHash("WalkLeft");
        public static readonly int Falling = Animator.StringToHash("Falling");
        public static readonly int Landing = Animator.StringToHash("Landing");
        public static readonly int Birth = Animator.StringToHash("Birth");
        public static readonly int Interacting = Animator.StringToHash("Interacting");
        public static readonly int InteractingMachine = Animator.StringToHash("RepairComputer");
        public static readonly int GoToBag = Animator.StringToHash("GoToBag");
    }
}
