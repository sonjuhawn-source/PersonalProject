using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class EnemyState : BaseState<EnemyBrain>
    {
        protected EnemyState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public virtual bool AllowsMovement => true;
        public virtual bool AllowsFacing => true;
        public virtual bool CanBeInterrupted => true;
    }
}
