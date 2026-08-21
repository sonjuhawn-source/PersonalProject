using UnityEngine;

namespace Game.Gameplay.Rooms
{
    internal class RunState
    {
        private readonly System.Random rng;
        private readonly RoomData[] rooms;

        internal int Seed { get; }
        internal int CurrentIndex { get; private set; }

        internal RoomData GetRoom(int i) => rooms[i];
        internal RoomData CurrentRoom => rooms[CurrentIndex];
        internal int RoomCount => rooms.Length;
        internal bool HasNext => CurrentIndex + 1 < rooms.Length;
        internal void Advance() => CurrentIndex += 1;

        public RunState(int seed, RoomData[] pool, RoomData bossRoom, int roomCount)
        {
            Seed = seed;
            rng = new System.Random(seed);
            int pickIndex;

            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("RunState: 방 풀이 비었다 — 방이 없는 런이 된다");
                roomCount = 0;
            }

            if (bossRoom == null)
                Debug.LogWarning("RunState: 보스방이 없다 — 런이 보스 없이 끝난다");

            int total = roomCount + (bossRoom != null ? 1 : 0);
            rooms = new RoomData[total];
            int prevIndex = -1;

            for (int i = 0; i < roomCount; i++)
            {
                if (prevIndex < 0 || pool.Length == 1)
                     pickIndex = rng.Next(0, pool.Length);
                else
                {
                    pickIndex = rng.Next(0, pool.Length - 1);
                    if (pickIndex >= prevIndex)
                        pickIndex += 1;
                }

                rooms[i] = pool[pickIndex];
                prevIndex = pickIndex;
            }
            if (bossRoom != null)
                rooms[total - 1] = bossRoom;
        }
    }
}
