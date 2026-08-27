using Game.Gameplay.Rooms;
using UnityEngine;

namespace Game.Gameplay
{
    public class FallKill : MonoBehaviour
    {
        [SerializeField]
        float margin = 3f;

        private Room room;
        private Health health;

        private void Start()
        {
            room = GetComponentInParent<Room>();
            health = GetComponent<Health>();

            if (room == null)
            {
                Debug.LogWarning($"{gameObject.name}: 부모에 Room 이 없다 — 방 경계를 몰라 떨어져도 안 죽는다", this);
                enabled = false;
            }
            if (health == null)
            {
                Debug.LogWarning($"{gameObject.name}: Health 가 없다 — 떨어져도 못 죽어 방을 클리어할 수 없다", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!room.TryGetBounds(out Bounds b))
                return;
            if (transform.position.y >= b.min.y - margin)
                return;

            enabled = false;
            health.TakeDamage(health.MaxHealth);
        }
    }
}
