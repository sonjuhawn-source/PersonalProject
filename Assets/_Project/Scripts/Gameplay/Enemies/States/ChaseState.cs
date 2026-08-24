using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class ChaseState : EnemyState
    {
        public ChaseState(StateMachine<EnemyBrain> machine) : base(machine) { }

        private bool wasMoving;

        public override void Enter()
        {
            wasMoving = true;
            Owner.PlayClip(clipMove);
        }

        public override void FixedTick()
        {
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
