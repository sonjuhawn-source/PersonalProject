using Game.Core;

namespace Game.Gameplay
{
    public abstract class PlayerState : BaseState<PlayerMovement>
    {
        protected PlayerState(StateMachine<PlayerMovement> machine) : base(machine) { }

        //#10 AttackState에서 실제로 사용 예정
        public virtual bool AllowsHorizontalControl => true;
        public virtual bool UsesGravity => true;
        public virtual bool AcceptsJumpInput => true;
    }
}