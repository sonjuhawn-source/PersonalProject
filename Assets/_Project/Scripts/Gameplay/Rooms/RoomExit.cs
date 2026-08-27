using System;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    public class RoomExit : MonoBehaviour
    {
        [SerializeField]
        private Collider2D trigger;
        [SerializeField]
        private SpriteRenderer portal;
        [SerializeField]
        private Color closedColor = new Color(0.35f, 0.35f, 0.5f, 1f);

        internal event Action Reached;

        private void Awake()
        {
            if (trigger == null)
            {
                Debug.LogWarning($"{gameObject.name}: trigger 미지정 — 클리어해도 출구가 안 열린다", this);
                enabled = false;
                return;
            }
            if (portal == null)
            {
                Debug.LogWarning($"{gameObject.name}: portal 미지정 — 출구가 열렸는지 눈에 안 보인다", this);
            }
            SetOpen(false);
        }

        internal void SetOpen(bool open)
        {
            if (trigger != null)
                trigger.enabled = open;
            if(portal != null)
                portal.color = open ? Color.white : closedColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            Reached?.Invoke();
        }

    }
}
