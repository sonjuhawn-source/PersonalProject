using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class TelegraphState : EnemyState
    {
        public TelegraphState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsFacing => false;
        public override bool CanBeInterrupted => Owner.Current == null || Owner.Current.Interruptible;

        private float elapsed;

        public override void Enter()
        {
            Owner.SetFacing(Owner.DirectionToTarget);
            Owner.PlayClip(Owner.Current.TelegraphStateName);
            Owner.SetTint(true);
            elapsed = 0;
        }

        public override void FixedTick()
        {
            elapsed += Time.fixedDeltaTime;
            if (elapsed >= Owner.Current.TelegraphTime)
                Machine.Change(Owner.Attack);
        }

        public override void Exit()
        {
            Owner.SetTint(false);
        }
    }
}
