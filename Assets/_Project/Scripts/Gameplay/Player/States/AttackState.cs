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

        private int comboIndex;
        private float lastAttackEndTime = float.NegativeInfinity;
        private bool comboQueued;
        private bool acceptingInput;

        public override void Enter()
        {
            if (Time.time - lastAttackEndTime > Owner.ComboResetTime)
                comboIndex = 0;

            Debug.Log($"플레이어: Attack {comboIndex + 1}타");

            cts = new CancellationTokenSource();
            RunTimeline().Forget();
        }

        public override void Tick()
        {
            if (acceptingInput && Owner.AttackPressed)
                comboQueued = true;
        }
        private async UniTaskVoid RunTimeline()
        {
            try
            {
                while (true)
                {
                    AttackData data = Owner.GetAttack(comboIndex);

                    comboQueued = false;
                    acceptingInput = false;
                    await UniTask.Delay(TimeSpan.FromSeconds(data.startup), cancellationToken: cts.Token);
                    Owner.HitBox.HitBoxActivate(Owner.BuildDamageInfo(data));
                    acceptingInput = true;
                    await UniTask.Delay(TimeSpan.FromSeconds(data.activeTime), cancellationToken: cts.Token);
                    Owner.HitBox.HitBoxDeactivate();
                    await UniTask.Delay(TimeSpan.FromSeconds(data.recovery), cancellationToken: cts.Token);
                    if (comboIndex >= Owner.ComboCount - 1)
                    {
                        comboIndex = 0;
                        break;
                    }

                    comboIndex++;

                    if (!comboQueued)
                        break;
                }

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
                acceptingInput = false;
            }


        }
        public override void Exit()
        {
            lastAttackEndTime = Time.time;

            if (cts == null)
                return;
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}
