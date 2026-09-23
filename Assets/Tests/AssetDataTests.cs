using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Enemies;
using Game.Gameplay.Rooms;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Tests
{
    public class AssetDataTests
    {
        private const string DataFolder = "Assets/_Project/Data";

        private static List<T> LoadAll<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}",
                                                      new[] { DataFolder });

            var list = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(a => a != null)
                .ToList();

            Assert.IsNotEmpty(list, $"{DataFolder} 에서 {typeof(T).Name} 을 하나도 못 찾았다");
            return list;
        }

        [Test]
        public void 모든_RoomData_에_roomPrefab_이_있다()
        {
            foreach (RoomData room in LoadAll<RoomData>())
                Assert.IsNotNull(room.RoomPrefab, $"{room.name}: roomPrefab 이 비었다");
        }

        [Test]
        public void 전투방에는_적_풀과_스폰_수가_있다()
        {
            foreach (RoomData room in LoadAll<RoomData>().Where(r => r.Type == RoomType.Combat))
            {
                Assert.Greater(room.EnemyPoolCount, 0, $"{room.name}: enemyPool 이 비었다");
                Assert.Greater(room.SpawnCount, 0, $"{room.name}: spawnCount 가 0 이다");
            }
        }

        [Test]
        public void 보상방에는_회복량과_무기_풀이_있다()
        {
            foreach (RoomData room in LoadAll<RoomData>().Where(r => r.Type == RoomType.Reward))
            {
                Assert.Greater(room.HealAmount, 0, $"{room.name}: healAmount 가 0 이다");
                Assert.Greater(room.RewardWeaponCount, 0, $"{room.name}: rewardWeapons 가 비었다");
            }
        }

        [Test]
        public void 모든_WeaponData_에_콤보와_오버라이드_컨트롤러가_있다()
        {
            foreach (WeaponData weapon in LoadAll<WeaponData>())
            {
                Assert.Greater(weapon.ComboCount, 0, $"{weapon.name}: combo 가 비었다");
                Assert.IsNotNull(weapon.OverrideController,
                                 $"{weapon.name}: overrideController 가 비었다");
            }
        }

        [Test]
        public void 투사체_패턴에는_투사체_프리팹이_있다()
        {
            foreach (AttackPattern p in LoadAll<AttackPattern>()
                                        .Where(p => p.Kind == AttackKind.Projectile))
            {
                Assert.IsNotNull(p.ProjectilePrefab, $"{p.name}: projectilePrefab 이 비었다");
            }
        }

        [Test]
        public void 모든_패턴이_minRange_보다_maxRange_가_크다()
        {
            foreach (AttackPattern p in LoadAll<AttackPattern>())
                Assert.Less(p.MinRange, p.MaxRange, $"{p.name}: 사거리가 뒤집혔다");
        }

        [Test]
        public void 보스_패턴이_0부터_10까지_구멍_없이_덮는다()
        {
            List<AttackPattern> boss = LoadAll<AttackPattern>()
                .Where(p => p.name.Contains("Ogre"))
                .OrderBy(p => p.MinRange)
                .ToList();

            Assert.AreEqual(3, boss.Count, "보스 패턴이 3개가 아니다");
            Assert.LessOrEqual(boss[0].MinRange, 0f, "0 거리에서 쓸 패턴이 없다");

            for (int i = 1; i < boss.Count; i++)
            {
                Assert.GreaterOrEqual(boss[i - 1].MaxRange, boss[i].MinRange,
                    $"{boss[i - 1].name} 과 {boss[i].name} 사이에 구멍이 있다");
            }

            Assert.GreaterOrEqual(boss[boss.Count - 1].MaxRange, 10f,
                "보스 감지 범위(10) 끝까지 닿는 패턴이 없다");
        }

        [Test]
        public void 근접_패턴_둘이_겹치고_가중치가_3대1_이다()
        {
            AttackPattern bite = LoadAll<AttackPattern>().First(p => p.name.Contains("Bite"));
            AttackPattern lunge = LoadAll<AttackPattern>().First(p => p.name.Contains("Lunge"));

            Assert.Less(lunge.MinRange, bite.MaxRange, "겹치는 구간이 없다");
            Assert.AreEqual(3f, bite.Weight / lunge.Weight, 0.001f, "가중치 비가 3:1 이 아니다");
        }

        [Test]
        public void 근접은_높이_제한이_있고_원거리는_없다()
        {
            foreach (AttackPattern p in LoadAll<AttackPattern>())
            {
                if (p.Kind == AttackKind.Projectile)
                    Assert.GreaterOrEqual(p.MaxHeightDiff, 10f,
                        $"{p.name}: 원거리인데 높이 제한이 걸려 있다");
                else
                    Assert.Less(p.MaxHeightDiff, 2f,
                        $"{p.name}: 근접인데 높이 제한이 사실상 없다");
            }
        }
    }
}