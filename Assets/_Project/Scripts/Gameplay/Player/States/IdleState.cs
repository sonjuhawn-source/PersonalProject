using Game.Core;

namespace Game.Gameplay
{
    public class IdleState : PlayerState
    {
        public IdleState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter()
        {
            Owner.PlayClip(clipIdle);
        }

        public override void Tick()
        {
            if (Owner.AttackPressed)
            {
                Machine.Change(Owner.Attack);
            }
        }

        public override void FixedTick()
        {
            if (!Owner.IsGrounded)
            {
                Machine.Change(Owner.Fall);
                return;
            }

            if (Owner.HasMoveInput)
            {
                Machine.Change(Owner.Move);
                return;
            }
        }
    }
}