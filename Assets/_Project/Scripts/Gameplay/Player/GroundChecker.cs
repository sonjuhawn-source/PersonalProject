using UnityEngine;

namespace Game.Gameplay
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] 
        Vector2 boxSize = new Vector2(0.8f, 0.1f);     // 발밑 박스 크기 (가로는 콜라이더보다 살짝 좁게)
        [SerializeField] 
        float offsetY = 0.02f;     // 콜라이더 발밑까지의 거리
        [SerializeField] 
        LayerMask groundMask;  // 인스펙터에서 Ground 선택

        public bool IsGrounded { get; private set; }

        private Collider2D bodyCollider;

        private void Awake()
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        void FixedUpdate()
        {
            IsGrounded = Physics2D.OverlapBox(FootCenter, boxSize , 0f, groundMask);
        }

        void OnDrawGizmosSelected()
        {
            if (ResolveCollider() == null) return;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(FootCenter, boxSize);
        }

        Vector2 FootCenter
        {
            get
            {
                Bounds b = ResolveCollider().bounds;
                return new Vector2(b.center.x, b.min.y - offsetY);
            }
        }

        Collider2D ResolveCollider()
        {
            if (bodyCollider == null)
                bodyCollider = GetComponent<Collider2D>();
            return bodyCollider;
        }
    }
}