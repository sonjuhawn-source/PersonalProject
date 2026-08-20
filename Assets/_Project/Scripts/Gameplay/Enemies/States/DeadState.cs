using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class DeadState : EnemyState
    {
        public DeadState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsMovement => false;

        public override bool AllowsFacing => false;
        public override bool CanBeInterrupted => false;

        public override void Enter()
        {
            Owner.Stop();
        }
    }
}
