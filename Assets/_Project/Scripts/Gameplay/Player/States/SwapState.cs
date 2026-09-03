using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class SwapState : PlayerState
    {
        public SwapState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override bool AllowsHorizontalControl => false;
        public override bool AcceptsJumpInput => false;
        public override bool AllowsFacingChange => false;
        public override bool AcceptsSwapInput => false;
        public override bool UsesGravity => false;

        private float remaining;

        public override void Enter()
        {
            Owner.PlayClip(clipJump, normalizedTime: 1);
            SfxPlayer.Play(Owner.SwapClips);
            remaining = Owner.DashTime;
            Owner.ApplyDash();
            Owner.BeginIFrame();
        }

        public override void Tick()
        {
            remaining -= Time.deltaTime;
            if (remaining > 0)
                return;
            if (Owner.IsGrounded)
                Machine.Change(Owner.Idle);
            else
                Machine.Change(Owner.Fall);
        }
    }
}
