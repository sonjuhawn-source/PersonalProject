using Game.Core;

namespace Game.Gameplay.Enemies
{
    public class IdleState : EnemyState
    {
        public IdleState(StateMachine<EnemyBrain> machine) : base(machine) { }
    }
}
