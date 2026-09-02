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
        public override bool AllowsHorizontalControl => !startedGrounded;
        public override bool AllowsFacingChange => !facingLocked;

        private CancellationTokenSource cts;

        private WeaponData lastWeapon;
        private int comboIndex;
        private float lastAttackEndTime = float.NegativeInfinity;
        private bool comboQueued;
        private bool acceptingInput;
        private bool facingLocked;
        private bool startedGrounded;

        public override void Enter()
        {
            if (lastWeapon != Owner.CurrentWeapon)
                comboIndex = 0;
            lastWeapon = Owner.CurrentWeapon;

            startedGrounded = Owner.IsGrounded;

            if (Time.time - lastAttackEndTime > Owner.ComboResetTime
           || comboIndex >= Owner.ComboCount)
                comboIndex = 0;

            // 씬 리로드는 Exit() 없이 파괴한다. UniTask.Delay 는 PlayerLoop 에 붙어 있어
            // GameObject 가 죽어도 계속 돌기 때문에, 오브젝트 수명에 토큰을 묶어야 한다.
            cts = CancellationTokenSource.CreateLinkedTokenSource(
                Owner.GetCancellationTokenOnDestroy());
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
                    Owner.PlayClip(Owner.AttackStateName, speed: (Owner.ClipWindup / data.startup));
                    SfxPlayer.Play(Owner.CurrentWeapon.AttackClips);
                    if(startedGrounded)
                        Owner.ApplyForward(data.startup);

                    comboQueued = false;
                    acceptingInput = false;
                    await UniTask.Delay(TimeSpan.FromSeconds(data.startup), cancellationToken: cts.Token);
                    if (Owner.Kind == AttackKind.Melee)
                        Owner.HitBox.HitBoxActivate(Owner.BuildDamageInfo(data));
                    else
                        Owner.FireProjectile(data);
                    acceptingInput = true;
                    facingLocked = true;
                    await UniTask.Delay(TimeSpan.FromSeconds(data.activeTime), cancellationToken: cts.Token);
                    Owner.HitBox.HitBoxDeactivate();
                    facingLocked = false;
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
                // ?. 는 C# 의 진짜 null 만 본다 — Unity 가 오버로드한 == 를 안 타므로
                // 파괴된 컴포넌트를 살아 있다고 보고 호출한다. != 를 써야 파괴가 잡힌다.
                if (Owner != null && Owner.HitBox != null)
                    Owner.HitBox.HitBoxDeactivate();
                acceptingInput = false;
                facingLocked = false;
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
