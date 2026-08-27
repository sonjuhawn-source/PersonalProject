using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class DeadState : EnemyState
    {
        public DeadState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsFacing => false;
        public override bool CanBeInterrupted => false;

        private float elapsed;

        public override void Enter()
        {
            Owner.Stop();
            Owner.PlayClip(clipDeath);
            elapsed = 0;
        }

        public override void FixedTick()
        {
            elapsed += Time.fixedDeltaTime;
            if (elapsed >= Owner.DeathDelay)
                Owner.Despawn();
        }
    }
}
