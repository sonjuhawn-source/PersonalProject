using Game.Core;

namespace Game.Gameplay.Enemies
{
    public abstract class EnemyState : BaseState<EnemyBrain>
    {
        protected EnemyState(StateMachine<EnemyBrain> machine) : base(machine) { }

        protected const string clipIdle = "Idle";
        protected const string clipMove = "Run";
        protected const string clipHit = "Hit";
        protected const string clipDeath = "Death";

        public virtual float MoveDirection => 0f;
        public virtual bool AllowsFacing => true;
        public virtual bool CanBeInterrupted => true;
    }
}
