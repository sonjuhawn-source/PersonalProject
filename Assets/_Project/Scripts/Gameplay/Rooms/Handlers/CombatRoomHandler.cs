using Game.Gameplay.Enemies;
using Game.Gameplay.Run;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Gameplay.Rooms
{
    internal class CombatRoomHandler : IRoomHandler
    {
        private const int HealPerKill = 1;
        private int remaining;
        private RunState run;
        private Health playerHealth;
        private readonly List<Health> subscribed = new List<Health>();

        public event Action Cleared;
        public bool EndsRun => false;

        public void Enter(Room room, RoomData data, RunState run)
        {
            this.run = run;
            remaining = 0;

            if (data.EnemyPoolCount == 0)
            {
                Debug.LogWarning($"{room.name}: enemyPool 이 비었다 — 전투방인데 스폰할 적이 없어 빈 방으로 통과한다", room);
                Cleared?.Invoke();
                return;
            }
            if (room.SpawnPointCount == 0)
            {
                Debug.LogWarning($"{room.name}: 스폰 포인트가 없다 — 전투방인데 적을 놓을 자리가 없어 빈 방으로 통과한다", room);
                Cleared?.Invoke(); 
                return;
            }
            int count = data.SpawnCount;
            if (count > room.SpawnPointCount)
            {
                Debug.LogWarning($"{room.name}: spawnCount {data.SpawnCount} 가 포인트 수 {room.SpawnPointCount} 를 넘어 깎았다", room);
                count = room.SpawnPointCount;
            }

            GameObject found = GameObject.FindWithTag("Player");
            playerHealth = found != null ? found.GetComponent<Health>() : null;

            for (int i = 0; i < count; i++)
            {
                var prefab = data.GetEnemy(Random.Range(0, data.EnemyPoolCount));
                Transform point = room.GetSpawnPoint(i);
                EnemyBrain enemy = Object.Instantiate(prefab, point.position, Quaternion.identity, room.transform);
                Health health = enemy.GetComponent<Health>();

                if (health == null)
                {
                    Debug.LogWarning($"{prefab.name}: Health 가 없다 — 죽일 수 없어 클리어 판정에서 뺀다", room);
                    continue;
                }

                health.Died += OnEnemyDied;
                subscribed.Add(health);
                remaining += 1;
            }

            if (remaining == 0)
                Cleared?.Invoke();
        }

        private void OnEnemyDied()
        {
            run?.AddKills(1);
            playerHealth?.Heal(HealPerKill);
            remaining -= 1;

            if (remaining <= 0)
                Cleared?.Invoke();
        }

        public void Exit()
        {
            foreach(var h in subscribed)
                h.Died -= OnEnemyDied;
            playerHealth = null;
            subscribed.Clear();
        }
    }
}
