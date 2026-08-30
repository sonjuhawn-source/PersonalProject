using Game.Gameplay.Enemies;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    [CreateAssetMenu(menuName = "Room/Room Data")]
    public class RoomData : ScriptableObject
    {
        [SerializeField]
        private RoomType type = RoomType.Combat;
        [SerializeField]
        private int difficulty = 1;
        [SerializeField]
        private Room roomPrefab;
        [SerializeField]
        private EnemyBrain[] enemyPool;
        [SerializeField]
        private int spawnCount = 1;
        [SerializeField]
        private int healAmount = 10;
        [SerializeField]
        private WeaponData[] rewardWeapons;

        internal RoomType Type => type;
        internal int Difficulty => difficulty;
        internal Room RoomPrefab => roomPrefab;
        internal int EnemyPoolCount => enemyPool == null ? 0 : enemyPool.Length;
        internal EnemyBrain GetEnemy(int i) => enemyPool[i];
        internal int SpawnCount => spawnCount;
        internal int HealAmount => healAmount;
        internal int RewardWeaponCount => rewardWeapons == null ? 0 : rewardWeapons.Length;
        internal WeaponData GetRewardWeapon(int i) => rewardWeapons[i];

    }
}
