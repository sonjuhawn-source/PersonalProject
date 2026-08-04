using Game.Core;

namespace Game.Gameplay
{
    public abstract class PlayerState : BaseState<PlayerMovement>
    {
        protected PlayerState(StateMachine<PlayerMovement> machine) : base(machine) { }

        protected const string clipIdle = "Idle";
        protected const string clipMove = "Run";
        protected const string clipJump = "Jump";
        protected const string clipAttack = "AttackSlash";

        public virtual bool AllowsHorizontalControl => true;
        public virtual bool UsesGravity => true;
        public virtual bool AcceptsJumpInput => true;
        public virtual bool AllowsFacingChange => AllowsHorizontalControl;
    }
}