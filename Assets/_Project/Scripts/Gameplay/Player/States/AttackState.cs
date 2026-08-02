using Cysharp.Threading.Tasks;
using Game.Core;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Gameplay
{
    public class AttackState : PlayerState
    {
        public AttackState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override bool AcceptsJumpInput => false;
        public override bool AllowsHorizontalControl => false;

        private CancellationTokenSource cts;

        public override void Enter()
        {
            Debug.Log("플레이어: Attack");
            cts = new CancellationTokenSource();
            RunTimeline().Forget();
        }


        private async UniTaskVoid RunTimeline()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Owner.AttackStartup), cancellationToken: cts.Token);
                Owner.HitBox.HitBoxActivate(Owner.BuildDamageInfo());
                await UniTask.Delay(TimeSpan.FromSeconds(Owner.AttackActive), cancellationToken: cts.Token);
                Owner.HitBox.HitBoxDeactivate();
                await UniTask.Delay(TimeSpan.FromSeconds(Owner.AttackRecovery), cancellationToken: cts.Token);
                if (Owner.IsGrounded)
                    Machine.Change(Owner.Idle);
                else
                    Machine.Change(Owner.Fall);
            }
            catch(OperationCanceledException)
            {

            }
            finally
            {
                Owner.HitBox.HitBoxDeactivate();
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
