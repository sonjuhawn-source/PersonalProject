using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class StaggerState : EnemyState
    {
        public StaggerState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsFacing => false;
        public override bool CanBeInterrupted => false;

        private float elapsed;

        public override void Enter()
        {
            Owner.PlayClip(clipHit);
            elapsed = 0;
        }

        public override void FixedTick()
        {
            elapsed += Time.fixedDeltaTime;
            if (elapsed >= Owner.StaggerTime)
                Machine.Change(Owner.Chase);
        }
    }
}
