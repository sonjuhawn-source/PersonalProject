using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class AttackState : EnemyState
    {
        public AttackState(StateMachine<EnemyBrain> machine) : base(machine)
        {
        }

    }
}
