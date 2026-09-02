using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class ChaseState : EnemyState
    {
        public ChaseState(StateMachine<EnemyBrain> machine) : base(machine) { }
        
        public override float MoveDirection
        {
            get
            {
                if (Owner.KeepDistance <= 0f)
                    return Owner.DirectionToTarget;

                float distance = Owner.DistanceToTarget;

                if (distance < Owner.KeepDistance)
                    return -Owner.DirectionToTarget;

                if (distance > Owner.StandRange)
                    return Owner.DirectionToTarget;

                return 0f;
            }
        }
        public override bool AllowsFacing => false;

        private bool wasMoving;

        public override void Enter()
        {
            wasMoving = true;
            Owner.SetFacing(Owner.DirectionToTarget);
            Owner.PlayClip(clipMove);
        }

        public override void FixedTick()
        {
            Owner.SetFacing(Owner.DirectionToTarget);

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
