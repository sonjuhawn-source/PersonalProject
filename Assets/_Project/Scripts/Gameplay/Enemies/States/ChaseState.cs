using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class ChaseState : EnemyState
    {
        public ChaseState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override void Enter()
        {
            Owner.PlayClip(clipMove);
        }

        public override void FixedTick()
        {
            if (Owner.TrySelectPattern())
                Machine.Change(Owner.Telegraph);
        }
    }
}
