using UnityEngine;

namespace Game.Gameplay.Rooms
{
    internal static class RoomHandlerFactory
    {
        internal static IRoomHandler Create(RoomData data)
        {
            switch (data.Type)
            {
                case RoomType.Combat: return new CombatRoomHandler();
                case RoomType.Reward: return new RewardRoomHandler();
                case RoomType.Boss: return new BossRoomHandler();
                default:
                    Debug.LogWarning($"{data.name}: {data.Type} 에 대응하는 핸들러가 없다 — 전투 핸들러로 폴백한다", data);
                    return new CombatRoomHandler();
            }
        }
    }
}
