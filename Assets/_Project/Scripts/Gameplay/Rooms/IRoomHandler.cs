using Game.Gameplay.Run;
using System;

namespace Game.Gameplay.Rooms
{
    internal interface IRoomHandler
    {
        event Action Cleared;
        void Enter(Room room, RoomData data, RunState run);
        void Exit();
    }
}
