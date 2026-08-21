using System;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    public class StageRunner : MonoBehaviour
    {
        [SerializeField]
        private RoomData[] roomPool;
        [SerializeField]
        private RoomData bossRoom;
        [SerializeField]
        private int roomCount = 10;
        [SerializeField]
        private int seed = -1;
        [SerializeField]
        private CameraFollow cameraFollow;

        private Transform player;
        private RunState run;
        private Room currentRoom;
        private IRoomHandler handler;

        private void Start()
        {
            if (cameraFollow == null)
            {
                Debug.LogWarning($"{gameObject.name}: cameraFollow 미지정 — 카메라가 방을 따라가지 못한다", this);
                enabled = false;
                return;
            }

            var found = GameObject.FindWithTag("Player");
            if (found == null)
            {
                Debug.LogWarning($"{gameObject.name}: Player 태그 오브젝트가 없다 — 방에 넣을 대상이 없어 런을 시작할 수 없다", this);
                enabled = false;
                return;
            }
            player = found.transform;

            int actualSeed = seed < 0 ? Environment.TickCount : seed;
            run = new RunState(actualSeed, roomPool, bossRoom, roomCount);

            if (run.RoomCount == 0)
            {
                Debug.LogWarning($"{gameObject.name}: 방이 0개다 — 런을 시작할 수 없다", this);
                enabled = false;
                return;
            }

            Debug.Log($"런 시작 — 시드 {actualSeed}, 방 {run.RoomCount}개");

            EnterRoom(Vector3.zero);
        }

        private void EnterRoom(Vector3 seam)
        {
            var data = run.CurrentRoom;
            if (data == null)
            {
                Debug.LogWarning($"{gameObject.name}: {run.CurrentIndex} 번째 방 데이터가 비었다 — roomPool 에 빈 칸이 있다", this);
                enabled = false;
                return;
            }
            if (data.RoomPrefab == null)
            {
                Debug.LogWarning($"{data.name}: roomPrefab 이 비었다 — 방을 만들 수 없다", data);
                enabled = false;
                return;
            }
            if (data.RoomPrefab.Entry == null)
            {
                Debug.LogWarning($"{data.RoomPrefab.name}: Entry 가 없다 — 방을 놓을 위치를 계산할 수 없다", data.RoomPrefab);
                enabled = false;
                return;
            }

            Vector3 pos = seam - data.RoomPrefab.Entry.localPosition;
            Room next = Instantiate(data.RoomPrefab, pos, Quaternion.identity);

            player.position = next.Entry.position;
            cameraFollow.SetRoom(next);

            if (next.ExitTrigger == null)
            {
                Debug.LogWarning($"{next.name}: exitTrigger 가 없다 — 클리어해도 나갈 수 없다", next);
                enabled = false;
                return;
            }

            next.ExitTrigger.SetOpen(false);
            next.ExitTrigger.Reached += OnExitReached;

            if (currentRoom != null)
            {
                foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
                    Destroy(p.gameObject);
                Destroy(currentRoom.gameObject);
            }

            currentRoom = next;

            handler = RoomHandlerFactory.Create(data);
            handler.Cleared += OnCleared;
            handler.Enter(next, data);
        }

        private void OnCleared()
        {
            currentRoom.ExitTrigger.SetOpen(true);
        }

        private void OnExitReached()
        {
            currentRoom.ExitTrigger.Reached -= OnExitReached;
            handler.Cleared -= OnCleared;
            handler.Exit();

            if (!run.HasNext)
            {
                Debug.Log("런 종료 — 마지막 방을 통과했다");
                return;
            }

            Vector3 seam = currentRoom.Exit.position;
            run.Advance();
            EnterRoom(seam);
        }
    }
}
