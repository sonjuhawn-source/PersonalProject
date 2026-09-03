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

        // 보상 뽑기도 시드를 타야 같은 시드가 같은 런이 된다.
        // rng 자체를 넘기면 밖에서 순서를 흐트러뜨릴 수 있으므로 호출만 연다.
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

            // 매번 무작위로 뽑고 직전 방만 피하던 방식은 A B A C B 를 정상으로 만든다.
            // 시드 10만 개로 재보니 다섯 방이 전부 다른 경우가 9.4% 뿐이었다 —
            // 열 판에 아홉 판은 같은 방을 또 만난다. 난수는 균등한데 구조가 반복을 만들었다.
            //
            // 덱처럼 쓴다. 풀을 섞어 앞에서부터 꺼내면 풀 크기만큼은 전부 다른 방이 나온다.
            // 지금은 roomCount 와 풀이 둘 다 5라 한 런에 모든 방이 정확히 한 번씩 나오고,
            // 런 길이(적 41마리)의 분산도 0 이 된다.
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

        // 덱을 다시 채운다. roomCount 가 풀보다 크면 여러 번 돈다.
        // rng 로 섞으므로 같은 시드는 같은 순서를 만든다.
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

            // 덱 끝에서 꺼내므로 마지막 원소가 다음 방이다. 그것이 직전 방과 같으면
            // 덱 경계에서만 중복이 생긴다 — 앞의 것과 바꿔 피한다.
            // 풀이 1개면 피할 방법이 없고, 그때는 같은 방이 반복되는 것이 맞다.
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
