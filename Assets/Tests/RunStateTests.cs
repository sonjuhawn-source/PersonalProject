using Game.Gameplay.Rooms;
using Game.Gameplay.Run;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Tests
{
    public class RunStateTests
    {
        private RoomData[] pool;
        private RoomData boss;

        [SetUp]
        public void SetUp()
        {
            pool = new RoomData[5];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = ScriptableObject.CreateInstance<RoomData>();
                pool[i].name = ((char)('A' + i)).ToString();
            }

            boss = ScriptableObject.CreateInstance<RoomData>();
            boss.name = "Boss";
        }
        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < pool.Length; i++)
                Object.DestroyImmediate(pool[i]);

            Object.DestroyImmediate(boss);
        }

        private RunState MakeRun(int seed, int roomCount)
        {
            return new RunState(
                seed: seed,
                pool: pool,
                bossRoom: boss,
                roomCount: roomCount,
                rewardRoom: null,
                rewardEvery: 0);
        }

        [Test]
        public void 같은_시드는_같은_방_순서다()
        {
            RunState a = MakeRun(12345, 5);
            RunState b = MakeRun(12345, 5);

            Assert.AreEqual(a.RoomCount, b.RoomCount);

            for (int i = 0; i < a.RoomCount; i++)
                Assert.AreSame(a.GetRoom(i), b.GetRoom(i), $"{i} 번째 방이 다르다");
        }

        [Test]
        public void 한_런에_전투방이_겹치지_않는다()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                RunState run = MakeRun(seed, 5);

                List<RoomData> combat = new List<RoomData>();
                for (int i = 0; i < 5; i++)
                    combat.Add(run.GetRoom(i));

                CollectionAssert.AllItemsAreUnique(combat, $"seed {seed}");
            }
        }

        [Test]
        public void 덱_경계에서_같은_방이_연속되지_않는다()
        {
            for (int seed = 0; seed < 100; seed++)
            {
                RunState run = MakeRun(seed, 7);

                Assert.AreNotSame(run.GetRoom(4), run.GetRoom(5), $"seed {seed}");
            }
        }

        [Test]
        public void 시드가_다르면_순서도_달라진다()
        {
            var orders = new HashSet<string>();
            string last = "";

            for (int i = 0; i < 100; i++)
            {
                RunState run = MakeRun(i, 5);

                string order = "";
                for (int j = 0; j < 5; j++)
                    order += run.GetRoom(j).name;

                orders.Add(order);
                last = order;
            }

            Assert.Greater(orders.Count, 1, $"모든 시드가 같은 순서: {last}");
        }
    }
}
