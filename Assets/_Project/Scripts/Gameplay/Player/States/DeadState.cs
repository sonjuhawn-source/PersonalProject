using Game.Core;

namespace Game.Gameplay
{
    public class DeadState : PlayerState
    {
        public DeadState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override bool AllowsHorizontalControl => false;
        public override bool AcceptsJumpInput => false;
        public override bool AllowsFacingChange => false;
        public override bool AcceptsSwapInput => false;

        public override void Enter()
        {
            Owner.Stop();
            Owner.PlayClip(clipDeath);
        }
    }
}
