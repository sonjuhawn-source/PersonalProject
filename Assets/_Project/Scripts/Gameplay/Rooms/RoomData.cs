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

        internal RoomType Type => type;
        internal int Difficulty => difficulty;
        internal Room RoomPrefab => roomPrefab;
        internal EnemyBrain[] EnemyPool => enemyPool;
        internal int SpawnCount => spawnCount;
    }
}
