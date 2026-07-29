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

        [SerializeField]
        Collider2D bodyCollider;   // 접지 기준이 될 몸통 콜라이더. 비우면 GetComponent 로 폴백

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            if (bodyCollider == null)
                Debug.LogWarning($"{name}: bodyCollider 미지정 — GetComponent 폴백. 콜라이더가 둘 이상이면 접지 판정과 기즈모가 함께 어긋난다.", this);

            ResolveCollider();
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