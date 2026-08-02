using Game.Core;

namespace Game.Gameplay
{
    public abstract class PlayerState : BaseState<PlayerMovement>
    {
        protected PlayerState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public virtual bool AllowsHorizontalControl => true;
        public virtual bool UsesGravity => true;
        public virtual bool AcceptsJumpInput => true;
    }
}