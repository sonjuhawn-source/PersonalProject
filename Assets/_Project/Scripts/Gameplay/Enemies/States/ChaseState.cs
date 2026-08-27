using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class ChaseState : EnemyState
    {
        public ChaseState(StateMachine<EnemyBrain> machine) : base(machine) { }
        
        public override float MoveDirection => Owner.DirectionToTarget;

        private bool wasMoving;

        public override void Enter()
        {
            wasMoving = true;
            Owner.PlayClip(clipMove);
        }

        public override void FixedTick()
        {
            if (Owner.DistanceToTarget > Owner.LoseRange
                || Owner.HeightToTarget > Owner.LoseHeight)
            {
                Machine.Change(Owner.Idle);
                return;
            }

            if (Owner.TrySelectPattern())
            {
                Machine.Change(Owner.Telegraph);
                return;
            }

            if (Owner.IsAdvancing != wasMoving)
            {
                wasMoving = Owner.IsAdvancing;
                Owner.PlayClip(wasMoving ? clipMove : clipIdle);
            }
        }
    }
}
