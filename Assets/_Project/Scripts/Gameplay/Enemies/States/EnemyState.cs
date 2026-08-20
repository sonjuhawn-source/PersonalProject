using Game.Core;

namespace Game.Gameplay.Enemies
{
    public abstract class EnemyState : BaseState<EnemyBrain>
    {
        protected EnemyState(StateMachine<EnemyBrain> machine) : base(machine) { }

        protected const string clipIdle = "Idle";
        protected const string clipMove = "Run";
        protected const string clipHit = "Hit";

        public virtual bool AllowsMovement => true;
        public virtual bool AllowsFacing => true;
        public virtual bool CanBeInterrupted => true;
    }
}
