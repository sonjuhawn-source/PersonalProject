using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Gameplay
{
    public class EnemyDummy : MonoBehaviour
    {
        [SerializeField]
        HitBox hitBox;
        [SerializeField]
        Transform target;
        [SerializeField]
        SpriteRenderer sprite;
        [SerializeField]
        AttackData attack;
        [SerializeField]
        float moveSpeed = 2f;
        [SerializeField]
        float attackRange = 0.9f;
        [SerializeField]
        Color telegraphColor;

        private Rigidbody2D body;
        private Knockback knockback;
        private Color baseColor;

        private bool busy;
        private float facing = 1f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            knockback = GetComponent<Knockback>();
            baseColor = sprite.color;
            Run().Forget(); 
        }

        private void FixedUpdate()
        {
            if (knockback.IsActive)
                return;
            if (busy)
            {
                body.linearVelocityX = 0;
                return;
            }

            float dx = target.position.x - transform.position.x;
            if(InRange())
            {
                body.linearVelocityX = 0;
            }
            else
            {
                body.linearVelocityX = Mathf.Sign(dx) * moveSpeed;
                SetFacing(Mathf.Sign(dx));
            }
        }

        private async UniTaskVoid Run()
        {
            var token = this.GetCancellationTokenOnDestroy();

            while (true)
            {
                await UniTask.WaitUntil(() => InRange(), cancellationToken: token);

                SetFacing(Mathf.Sign(target.position.x - transform.position.x));
                busy = true;

                sprite.color = telegraphColor;              // 예고
                await UniTask.Delay(TimeSpan.FromSeconds(attack.startup), cancellationToken: token);
                sprite.color = baseColor;

                hitBox.HitBoxActivate(BuildDamageInfo());   // 판정
                await UniTask.Delay(TimeSpan.FromSeconds(attack.activeTime), cancellationToken: token);
                hitBox.HitBoxDeactivate();

                await UniTask.Delay(TimeSpan.FromSeconds(attack.recovery), cancellationToken: token);
                busy = false;
            }
        }

        private DamageInfo BuildDamageInfo()
        {
            return new DamageInfo(attack.damage,
                              Vector2.right * facing * attack.knockbackForce,
                              attack.hitStopTime,
                              gameObject,
                              attack.shakeStrength);
        }

        private bool InRange()
        {
            return Mathf.Abs(target.position.x - transform.position.x) <= attackRange;
        }

        private void SetFacing(float dir)
        {
            facing = dir;
            transform.localScale = new Vector3(dir, 1, 1);
        }
    }
}
