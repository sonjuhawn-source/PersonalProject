using Game.Gameplay.Enemies;
using Game.Gameplay.Run;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Gameplay.Rooms
{
    internal class BossRoomHandler : IRoomHandler
    {
        private Health bossHealth;
        private RunState run;

        public event Action Cleared;
        public bool EndsRun => true;

        public void Enter(Room room, RoomData data, RunState run)
        {
            this.run = run;

            if (data.EnemyPoolCount == 0)
            {
                Debug.LogWarning($"{room.name}: 보스방인데 enemyPool 이 비었다 — 보스가 없어 빈 방으로 통과한다", room);
                Cleared?.Invoke();
                return;
            }
            if (room.SpawnPointCount == 0)
            {
                Debug.LogWarning($"{room.name}: 스폰 포인트가 없다 — 보스를 놓을 자리가 없다", room);
                Cleared?.Invoke();
                return;
            }
            if (data.EnemyPoolCount > 1)
            {
                Debug.LogWarning($"{room.name}: 보스가 여럿이다 — 첫 칸만 쓴다", room);
            }
            EnemyBrain prefab = data.GetEnemy(0);
            Transform point = room.GetSpawnPoint(0);
            EnemyBrain boss = Object.Instantiate(prefab, point.position, Quaternion.identity, room.transform);

            bossHealth = boss.GetComponent<Health>();
            if (bossHealth == null)
            {
                Debug.LogWarning($"{room.name}: 보스에 Health 가 없다 — 죽일 수 없어 런이 안 끝난다", room);
                Cleared?.Invoke();
                return;
            }

            bossHealth.Died += OnBossDied;
        }

        private void OnBossDied()
        {
            run?.AddKills(1);
            Cleared?.Invoke();
        }

        public void Exit()
        {
            if (bossHealth != null)
            {
                bossHealth.Died -= OnBossDied;
                bossHealth = null;
            }
        }
    }
}
