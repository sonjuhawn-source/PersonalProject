using Game.Core;

namespace Game.Gameplay
{
    public class FallState : PlayerState
    {
        public FallState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter() 
        {
            Owner.PlayClip(clipJump, 1);
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
            if (Owner.IsGrounded)
            {
                if (Owner.HasMoveInput)
                {
                    Machine.Change(Owner.Move);
                }
                else
                {
                    Machine.Change(Owner.Idle);
                }
            }
        }
    }
}