using System;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    public class RoomExit : MonoBehaviour
    {
        [SerializeField]
        private Collider2D trigger;

        internal event Action Reached;

        private void Awake()
        {
            if (trigger == null)
            {
                Debug.LogWarning($"{gameObject.name}: trigger 미지정 — 클리어해도 출구가 안 열린다", this);
                enabled = false;
                return;
            }
            trigger.enabled = false;
        }

        internal void SetOpen(bool open)
        {
            if (trigger == null)
                return;
            trigger.enabled = open;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) 
                return;

            Reached?.Invoke();
        }
        
    }
}
