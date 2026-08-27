using Game.Gameplay.Run;
using System;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    internal class RewardRoomHandler : IRoomHandler
    {
        public event Action Cleared;
        public bool EndsRun => false;

        public void Enter(Room room, RoomData data, RunState run)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if(found == null)
            {
                Debug.LogWarning($"{room.name}: 플레이어가 없다 — 회복을 줄 대상이 없다", room);
                Cleared?.Invoke();
                return;
            }
            Health health = found.GetComponent<Health>();
            if(health == null)
            {
                Debug.LogWarning($"{room.name}: Health 가 없다 — 회복할 수 없다", room);
                Cleared?.Invoke();
                return;
            }

            int before = health.CurrentHealth;
            health.Heal(data.HealAmount);
            Debug.Log($"{room.name}: 보상방 — HP {before} → {health.CurrentHealth}", room);
            Cleared?.Invoke();
        }

        public void Exit() { }
    }
}
