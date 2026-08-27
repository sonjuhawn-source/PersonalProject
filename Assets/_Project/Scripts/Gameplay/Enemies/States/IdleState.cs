using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class IdleState : EnemyState
    {
        public IdleState(StateMachine<EnemyBrain> machine) : base(machine) { }

        private float patrolDir = 1f;
        public override float MoveDirection => Owner.Patrols ? patrolDir : 0f;

        public override bool AllowsFacing => false;

        public override void Enter()
        {
            Owner.PlayClip(Owner.Patrols ? clipMove : clipIdle);
            Owner.SetFacing(patrolDir);
        }

        public override void FixedTick()
        {
            if (Owner.DistanceToTarget <= Owner.DetectRange
                && Owner.HeightToTarget <= Owner.DetectHeight)
            {
                Machine.Change(Owner.Chase);
                return;
            }

            if (!Owner.CanAdvance(patrolDir) && Owner.CanAdvance(-patrolDir))
            {
                patrolDir = -patrolDir;
                Owner.SetFacing(patrolDir);
            }
        }
    }
}
