using UnityEngine;
using Game.Gameplay.Run;
using System;

namespace Game.Gameplay.Rooms
{
    internal class BossRoomHandler : IRoomHandler
    {
        public event Action Cleared;

        public void Enter(Room room, RoomData data, RunState run)
        {
            Debug.Log($"{room.name}: 보스방 — 구현은 W5. 즉시 통과한다", room);
            Cleared?.Invoke();
        }

        public void Exit()
        {
            
        }
    }
}
