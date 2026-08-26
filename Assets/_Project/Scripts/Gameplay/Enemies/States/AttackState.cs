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
            // 씬 리로드는 Exit() 없이 파괴한다. UniTask.Delay 는 PlayerLoop 에 붙어 있어
            // GameObject 가 죽어도 계속 돌기 때문에, 오브젝트 수명에 토큰을 묶어야 한다.
            cts = CancellationTokenSource.CreateLinkedTokenSource(
                Owner.GetCancellationTokenOnDestroy());
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
                // ?. 는 C# 의 진짜 null 만 본다 — Unity 가 오버로드한 == 를 안 타므로
                // 파괴된 컴포넌트를 살아 있다고 보고 호출한다. != 를 써야 파괴가 잡힌다.
                if (Owner != null && Owner.HitBox != null)
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
