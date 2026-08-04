using UnityEngine;

namespace Game.Gameplay
{
    public class Knockback : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody2D body;
        [SerializeField]
        private float knockbackTime = 0.2f;

        private float remaining;
        private Vector2 initial;

        internal bool IsActive => remaining > 0;

        private void Awake()
        {
            if(body == null)
            {
                Debug.LogWarning($"{gameObject.name}: Rigidbody가 배선되지 않았습니다", this);
                body = GetComponent<Rigidbody2D>();
            }
        }

        internal void Apply(Vector2 impulse)
        {
            initial = impulse;
            remaining = knockbackTime;
        }

        private void FixedUpdate()
        {
            if (remaining <= 0)
                return;
            float t = Mathf.Max(remaining, 0) / knockbackTime;
            remaining -= Time.fixedDeltaTime;
            body.linearVelocityX = initial.x * t;   //띄우지 않음
        }
    }
}
