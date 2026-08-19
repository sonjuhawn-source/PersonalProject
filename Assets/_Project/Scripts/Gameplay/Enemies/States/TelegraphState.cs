using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class TelegraphState : EnemyState
    {
        public TelegraphState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsMovement => false;
        public override bool AllowsFacing => false;

        private float elapsed;

        public override void Enter()
        {
            Owner.SetFacing(Owner.DirectionToTarget);
            Owner.PlayClip(Owner.Current.TelegraphStateName);
            elapsed = 0;
        }

        public override void FixedTick()
        {
            elapsed += Time.fixedDeltaTime;
            if (elapsed >= Owner.Current.TelegraphTime)
                Machine.Change(Owner.Attack);
        }
    }
}
