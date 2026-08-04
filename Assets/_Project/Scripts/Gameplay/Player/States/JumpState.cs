using Game.Core;

namespace Game.Gameplay
{
    public class JumpState : PlayerState
    {
        public JumpState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter() 
        {
            Owner.PlayClip(clipJump);
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
            if (Owner.IsFalling)
            {
                Machine.Change(Owner.Fall);
            }
        }
    }
}