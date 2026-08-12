using UnityEngine;

namespace Game.Gameplay
{
    public class VelocityImpulse : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody2D body;
        [SerializeField]
        private float defaultDuration = 0.2f;

        private float remaining;
        private float total;
        private Vector2 initial;

        internal bool IsActive => remaining > 0;

        private void Awake()
        {
            if (body == null)
            {
                Debug.LogWarning($"{gameObject.name}: Rigidbody가 배선되지 않았습니다", this);
                body = GetComponent<Rigidbody2D>();
            }
        }
        internal void Apply(Vector2 impulse) => Apply(impulse, defaultDuration);
        internal void Apply(Vector2 impulse, float duration)
        {
            initial = impulse;
            remaining = total = duration;
        }

        private void FixedUpdate()
        {
            if (remaining <= 0)
                return;
            float t = remaining / total;
            remaining -= Time.fixedDeltaTime;
            body.linearVelocityX = initial.x * t;   //띄우지 않음
        }
    }
}
