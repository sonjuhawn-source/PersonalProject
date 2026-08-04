using Game.Core;

namespace Game.Gameplay
{
    public class MoveState : PlayerState
    {
        public MoveState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter() 
        {
            Owner.PlayClip(clipMove);
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

            if (!Owner.HasMoveInput)
            {
                Machine.Change(Owner.Idle);
                return;
            }
        }
    }
}