using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class IdleState : EnemyState
    {
        public IdleState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsMovement => false;
        public override bool AllowsFacing => false;

        public override void Enter()
        {
            Owner.PlayClip(clipIdle);
        }

        public override void FixedTick()
        {
            if (Owner.DistanceToTarget <= Owner.DetectRange)
                Machine.Change(Owner.Chase);
        }
    }
}
