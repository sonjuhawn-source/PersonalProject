using Game.Gameplay.Rooms;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Run
{
    internal class RunState
    {
        private readonly System.Random rng;
        private readonly RoomData[] rooms;

        private WeaponData[] weapons = System.Array.Empty<WeaponData>();

        internal int Seed { get; }
        internal int CurrentIndex { get; private set; }

        internal RoomData GetRoom(int i) => rooms[i];
        internal RoomData CurrentRoom => rooms[CurrentIndex];
        internal int RoomCount => rooms.Length;
        internal bool HasNext => CurrentIndex + 1 < rooms.Length;
        internal void Advance() => CurrentIndex += 1;

        internal int CurrentHealth { get; private set; }
        internal int KillCount { get; private set; }

        internal int WeaponCount => weapons.Length;
        internal WeaponData GetWeapon(int i) => weapons[i];

        internal void RecordHealth(int value) => CurrentHealth = value;
        internal void AddKills(int count) => KillCount += count;

        internal void RecordWeapons(WeaponData[] value)
        {
            weapons = value ?? System.Array.Empty<WeaponData>();
        }

        // 보상 뽑기도 시드를 타야 같은 시드가 같은 런이 된다.
        // rng 자체를 넘기면 밖에서 순서를 흐트러뜨릴 수 있으므로 호출만 연다.
        internal int NextInt(int minInclusive, int maxExclusive)
            => rng.Next(minInclusive, maxExclusive);

        // 재료를 다 갖고 있는 쪽이 만든다. 밖에서 조립하면 필드가 늘 때마다 조립부를 고친다.
        internal RunResult BuildResult(RunOutcome outcome)
        {
            return new RunResult(outcome, CurrentIndex + 1, KillCount, CurrentHealth);
        }

        public RunState(int seed, RoomData[] pool, RoomData bossRoom, int roomCount, RoomData rewardRoom, int rewardEvery)
        {
            Seed = seed;
            rng = new System.Random(seed);
            int pickIndex;
            int prevIndex = -1;

            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("RunState: 방 풀이 비었다 — 방이 없는 런이 된다");
                roomCount = 0;
            }

            if (bossRoom == null)
                Debug.LogWarning("RunState: 보스방이 없다 — 런이 보스 없이 끝난다");

            var list = new List<RoomData>();

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
                list.Add(pool[pickIndex]);
                prevIndex = pickIndex;

                bool lastCombat = (i == roomCount - 1);

                if (rewardRoom != null && rewardEvery > 0 && (i + 1) % rewardEvery == 0 && !lastCombat)
                    list.Add(rewardRoom);
            }

            if (bossRoom != null)
                list.Add(bossRoom);

            rooms = list.ToArray();
        }
    }
}
