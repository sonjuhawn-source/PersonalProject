using Game.Gameplay.Run;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        [SerializeField] 
        private float fallMargin = 3f;

        private Transform player;
        private Health playerHealth;
        private WeaponHolder playerWeapons;
        private RunState run;
        private Room currentRoom;
        private IRoomHandler handler;
        private bool runEnded;
        private GroundChecker playerGround;
        private Rigidbody2D playerBody;

        public event Action<string> RunEnded;

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
            playerGround = found.GetComponent<GroundChecker>();
            playerBody = found.GetComponent<Rigidbody2D>();
            if (playerGround == null)
            {
                Debug.LogWarning($"{gameObject.name}: GroundChecker 가 없다 — 떨어져도 돌아올 지점을 모른다", this);
            }
            if (playerBody == null)
            {
                Debug.LogWarning($"{gameObject.name}: Rigidbody2D 가 없다 — 되돌려도 낙하 속도가 남는다", this);
            }

            player = found.transform;
            playerHealth = found.GetComponent<Health>();
            playerWeapons = found.GetComponent<WeaponHolder>();

            if (playerHealth == null)
                Debug.LogWarning($"{gameObject.name}: 플레이어에 Health 가 없다 — 죽어도 런이 끝나지 않는다", this);
            else
                playerHealth.Died += OnPlayerDied;
            if (playerWeapons == null)
                Debug.LogWarning($"{gameObject.name}: 플레이어에 WeaponHolder 가 없다 — 런 결과에 무기가 안 남는다", this);

            int actualSeed = seed < 0 ? Environment.TickCount : seed;
            run = new RunState(actualSeed, roomPool, bossRoom, roomCount);

            if (run.RoomCount == 0)
            {
                Debug.LogWarning($"{gameObject.name}: 방이 0개다 — 런을 시작할 수 없다", this);
                enabled = false;
                return;
            }

            CaptureFromPlayer();

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
            playerGround?.ResetLastGrounded();

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
            handler.Enter(next, data, run);
        }

        // 런에 걸치는 값을 경계에서만 RunState 로 넘긴다.
        // 전투 중 주인은 Health · WeaponHolder 이고, 여기서는 회수만 한다.
        private void CaptureFromPlayer()
        {
            if (playerHealth != null)
                run.RecordHealth(playerHealth.CurrentHealth);
            if (playerWeapons != null)
                run.RecordWeapons(playerWeapons.SnapshotWeapons());
        }

        private void OnPlayerDied() => EndRun(RunOutcome.Died);

        // 끝나는 길이 둘이지만 결과를 만드는 곳은 하나다.
        // 각자 만들면 "클리어와 사망이 같은 화면, 결과만 다르다" 가 안 지켜진다.
        private void EndRun(RunOutcome outcome)
        {
            if (runEnded)
                return;
            runEnded = true;

            CaptureFromPlayer();
            RunResult result = run.BuildResult(outcome);

            string text = $"런 종료 ... {(outcome == RunOutcome.Cleared ? "클리어" : "사망")} · " +
              $"{result.Floor}층 · {result.Kills}킬 · HP {result.Health} · " +
              $"무기 {DescribeWeapons()}\n" +
              "R 을 누르면 다시 시작한다";

            Debug.Log(text);
            RunEnded?.Invoke(text);
        }

        // #103 이 재시작 버튼을 붙이면 이 폴링은 사라진다.
        // .inputactions 를 건드리지 않는 이유는 자산 저장과 생성 클래스 재생성이 따라오기 때문이다.
        private void Update()
        {
            if (!runEnded)
                return;
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Restart();
        }

        private void FixedUpdate()
        {
            if (runEnded || currentRoom == null || player == null)
                return;
            if (playerGround == null || playerBody == null)
                return;
            if (!currentRoom.TryGetBounds(out Bounds b)) 
                return;
            if (player.position.y >= b.min.y - fallMargin) 
                return;

            player.position = playerGround.LastGroundedPosition;
            playerBody.linearVelocity = Vector2.zero;
        }

        private void Restart()
        {
            // 빌드에는 도메인 리로드가 없어 static 이 살아남는다.
            // HitStop 이 timeScale 0 인 중에 끝났다면 씬만 다시 로드해서는 안 풀린다.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.Died -= OnPlayerDied;
        }

        // #103 이 이 로그를 결과 화면으로 대체한다.
        private string DescribeWeapons()
        {
            if (run.WeaponCount == 0)
                return "없음";

            var names = new string[run.WeaponCount];
            for (int i = 0; i < run.WeaponCount; i++)
            {
                var w = run.GetWeapon(i);
                names[i] = w != null ? w.name : "빈 칸";
            }
            return string.Join(" · ", names);
        }

        private void OnCleared()
        {
            if (handler.EndsRun)
            {
                EndRun(RunOutcome.Cleared);
                return;
            }
            currentRoom.ExitTrigger.SetOpen(true);
        }

        private void OnExitReached()
        {
            currentRoom.ExitTrigger.Reached -= OnExitReached;
            handler.Cleared -= OnCleared;
            handler.Exit();

            if (!run.HasNext)
            {
                EndRun(RunOutcome.Cleared);
                return;
            }

            CaptureFromPlayer();

            Vector3 seam = currentRoom.Exit.position;
            run.Advance();
            EnterRoom(seam);
        }
    }
}
