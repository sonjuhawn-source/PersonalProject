using UnityEngine;

namespace Game.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        private float speed = 12;
        [SerializeField]
        private float lifetime = 2;
        [SerializeField]
        private int pierce = 3;
        [SerializeField] 
        private LayerMask groundMask;

        private HitBox hitBox;
        private Rigidbody2D body;

        private void Awake()
        {
            hitBox = GetComponent<HitBox>();
            body = GetComponent<Rigidbody2D>();
            if(groundMask.value == 0)
            {
                Debug.LogWarning($"{gameObject.name}: groundMask 미지정 — 화살이 지형을 통과한다", this);
            }
        }

        private void FixedUpdate()
        {
            Vector2 v = body.linearVelocity;
            if (v.sqrMagnitude <= 0f) 
                return;

            float dist = v.magnitude * Time.fixedDeltaTime;
            if (Physics2D.Raycast(transform.position, v.normalized, dist, groundMask))
            {
                hitBox.HitBoxDeactivate();
                Destroy(gameObject);
            }
        }

        internal void Init(DamageInfo info, float facing)
        {
            hitBox.Hit += OnHit;
            hitBox.HitBoxActivate(info);

            body.linearVelocity = Vector2.right * facing * speed;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            transform.localScale = s;

            Destroy(gameObject, lifetime);
        }

        private void OnHit(HurtBox hurtbox)
        {
            pierce -= 1;
            if (pierce > 0)
                return;

            hitBox.HitBoxDeactivate();

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            hitBox.Hit -= OnHit;
        }
    }
}
