using Cysharp.Threading.Tasks;
using Game.Core;
using System;
using System.Threading;

namespace Game.Gameplay.Enemies
{
    public class AttackState : EnemyState
    {
        public AttackState(StateMachine<EnemyBrain> machine) : base(machine) { }

        public override bool AllowsMovement => false;
        public override bool AllowsFacing => false;
        public override bool CanBeInterrupted => false;

        private CancellationTokenSource cts;

        public override void Enter()
        {
            cts = new CancellationTokenSource();
            RunTimeline().Forget();
        }

        private async UniTaskVoid RunTimeline()
        {
            try
            {
                var pattern = Owner.Current;
                var data = pattern.Attack;

                Owner.PlayClip(pattern.AttackStateName);
                Owner.ApplyForward(data.startup);

                await UniTask.Delay(TimeSpan.FromSeconds(data.startup), cancellationToken: cts.Token);

                if(pattern.Kind == AttackKind.Melee)
                {
                    Owner.HitBox?.SetShape(pattern.HitboxOffset, pattern.HitboxSize);
                    Owner.HitBox?.HitBoxActivate(Owner.BuildDamageInfo(data));
                }
                else
                    Owner.FireProjectile(data);

                await UniTask.Delay(TimeSpan.FromSeconds(data.activeTime), cancellationToken: cts.Token);

                Owner.HitBox?.HitBoxDeactivate();
                Machine.Change(Owner.Recover);
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                Owner.HitBox?.HitBoxDeactivate();
            }
        }

        public override void Exit()
        {
            if (cts == null) 
                return;

            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}
