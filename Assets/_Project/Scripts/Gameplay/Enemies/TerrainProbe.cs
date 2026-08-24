using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class TerrainProbe : MonoBehaviour
    {
        [SerializeField]
        private LayerMask groundMask;
        [SerializeField]
        private float aheadDistance = 0.1f;
        [SerializeField]
        private float probeDown = 0.5f;
        [SerializeField]
        private float wallDistance = 0.05f;

        private Collider2D bodyCollider;


        private void Awake()
        {
            bodyCollider = GetComponent<Collider2D>();
            if (groundMask.value == 0)
            {
                Debug.LogWarning($"{gameObject.name}: groundMask 미지정 — 전방 지면을 못 찾아 모든 방향이 절벽으로 보이고 적이 움직이지 않는다", this);
            }
            if (bodyCollider == null)
            {
                Debug.LogWarning($"{gameObject.name}: bodyCollider null — 레이어 체크 불가함", this);
            }
        }

        private Vector2 GroundProbeOrigin(Bounds bounds, float dir, float distance)
        {
            return new Vector2(bounds.center.x + dir * (bounds.extents.x + distance),
                               bounds.min.y + 0.05f);
        }

        private Vector2 GroundProbeOrigin(Bounds bounds, float dir) => GroundProbeOrigin(bounds, dir, aheadDistance);

        internal bool HasGroundAt(float dir, float distance)
        {
            if (bodyCollider == null) 
                return true;

            var startPos = GroundProbeOrigin(bodyCollider.bounds, dir , distance);
            return Physics2D.Raycast(startPos, Vector2.down, probeDown, groundMask);
        }

        internal bool HasGroundAhead(float dir) => HasGroundAt(dir, aheadDistance);

        internal bool HasWallAhead(float dir)
        {
            if (bodyCollider == null) 
                return false;

            var halfWidth = bodyCollider.bounds.extents.x;
            var startPos = bodyCollider.bounds.center;
            var distance = halfWidth + wallDistance;
            return Physics2D.Raycast(startPos, Vector2.right * dir, distance, groundMask);
        }

        internal bool CanAdvance(float dir)
        {
            return HasGroundAhead(dir) && !HasWallAhead(dir);
        }

        private void OnDrawGizmos()
        {
            Collider2D body = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
            if (body == null) return;

            Bounds bounds = body.bounds;
            float wallLength = bounds.extents.x + wallDistance;

            for (int i = 0; i < 2; i++)
            {
                float dir = i == 0 ? -1f : 1f;

                Vector2 groundOrigin = GroundProbeOrigin(bounds, dir);
                bool hasGround = Physics2D.Raycast(groundOrigin, Vector2.down, probeDown, groundMask);
                Gizmos.color = hasGround ? Color.green : Color.red;
                Gizmos.DrawLine(groundOrigin, groundOrigin + Vector2.down * probeDown);
                Gizmos.DrawWireSphere(groundOrigin, 0.03f);

                Vector2 wallOrigin = bounds.center;
                bool hasWall = Physics2D.Raycast(wallOrigin, Vector2.right * dir, wallLength, groundMask);
                Gizmos.color = hasWall ? Color.red : Color.green;
                Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * dir * wallLength);
            }
        }
    }
}
