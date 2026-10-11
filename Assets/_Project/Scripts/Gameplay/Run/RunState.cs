using Game.Gameplay.Rooms;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Run
{
    internal class RunState
    {
        private readonly System.Random rng;
        private readonly RoomData[] rooms;

        private WeaponSnapshot[] weapons = Array.Empty<WeaponSnapshot>();

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
        internal WeaponSnapshot GetWeapon(int i) => weapons[i];

        internal void RecordHealth(int value) => CurrentHealth = value;
        internal void AddKills(int count) => KillCount += count;

        internal void RecordWeapons(WeaponSnapshot[] value)
        {
            weapons = value ?? System.Array.Empty<WeaponSnapshot>();
        }

        // rng 를 넘기면 밖에서 순서를 흐트러뜨린다. 호출만 연다.
        internal int NextInt(int minInclusive, int maxExclusive)
            => rng.Next(minInclusive, maxExclusive);

        // 재료를 다 갖고 있는 쪽이 만든다. 밖에서 조립하면 필드가 늘 때마다 조립부를 고친다.
        internal RunResult BuildResult(RunOutcome outcome)
        {
            return new RunResult(outcome, CurrentIndex + 1, KillCount, CurrentHealth, weapons);
        }

        public RunState(int seed, RoomData[] pool, RoomData bossRoom, int roomCount, RoomData rewardRoom, int rewardEvery)
        {
            Seed = seed;
            rng = new System.Random(seed);

            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("RunState: 방 풀이 비었다 — 방이 없는 런이 된다");
                roomCount = 0;
            }

            if (bossRoom == null)
                Debug.LogWarning("RunState: 보스방이 없다 — 런이 보스 없이 끝난다");

            var list = new List<RoomData>();

            // 직전 방만 피하면 A B A C B 가 정상이 된다 — 시드 10만 개에서 전부 다른 런이 9.4%.
            // 풀을 섞어 앞에서 꺼낸다. #135
            var deck = new List<RoomData>();
            RoomData previous = null;

            for (int i = 0; i < roomCount; i++)
            {
                if (deck.Count == 0)
                    Refill(deck, pool, previous);

                // 뒤에서 꺼낸다. 앞에서 꺼내면 매번 리스트가 밀린다.
                int last = deck.Count - 1;
                previous = deck[last];
                deck.RemoveAt(last);
                list.Add(previous);

                bool lastCombat = (i == roomCount - 1);

                if (rewardRoom != null && rewardEvery > 0 && (i + 1) % rewardEvery == 0 && !lastCombat)
                    list.Add(rewardRoom);
            }

            if (bossRoom != null)
                list.Add(bossRoom);

            rooms = list.ToArray();
        }

        // roomCount 가 풀보다 크면 여러 번 돈다. rng 로 섞어 같은 시드는 같은 순서.
        private void Refill(List<RoomData> deck, RoomData[] pool, RoomData previous)
        {
            deck.Clear();
            for (int i = 0; i < pool.Length; i++)
                deck.Add(pool[i]);

            // Fisher-Yates. 뒤에서부터 앞의 무작위 원소와 교환한다.
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                RoomData temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }

            // 덱 경계에서만 직전 방과 겹칠 수 있다 — 앞의 것과 바꿔 피한다. 풀이 1개면 못 피한다.
            int end = deck.Count - 1;
            if (previous != null && deck.Count > 1 && deck[end] == previous)
            {
                RoomData temp = deck[end];
                deck[end] = deck[0];
                deck[0] = temp;
            }
        }
    }
}
