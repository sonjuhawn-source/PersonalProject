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
            Debug.Log($"{room.name}: 보상방 — 구현은 W4. 즉시 통과한다", room);
            Cleared?.Invoke();
        }

        public void Exit() { }
    }
}
