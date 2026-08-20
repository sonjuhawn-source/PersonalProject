using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class RecoverState : EnemyState
    {
        public RecoverState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsMovement => false;
        public override bool AllowsFacing => false;

        private float elapsed;

        public override void Enter()
        {
            elapsed = 0;
        }

        public override void FixedTick()
        {
            elapsed += Time.fixedDeltaTime;
            if (elapsed >= Owner.Current.Attack.recovery)
                Machine.Change(Owner.Chase);
        }
    }
}
