using UnityEngine;

namespace Game.Gameplay.Rooms
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform target;
        [SerializeField]
        private Camera cam;
        [SerializeField]
        private Room currentRoom;

        internal void SetRoom(Room room) => currentRoom = room;

        private void Awake()
        {
            if(target == null)
            {
                Debug.LogWarning($"{gameObject.name}: target 미지정 - 카메라가 따라갈 대상이 없음", this);
                enabled = false;
                return;
            }
            if (cam == null)
            {
                Debug.LogWarning($"{gameObject.name}: cam 미지정 - 화면 크기를 몰라 경계 계산이 안 된다", this);
                enabled = false;
                return;
            }
            if (currentRoom == null)
            {
                Debug.LogWarning($"{gameObject.name}: currentRoom 미지정 - 방 경계를 몰라 추적을 멈춘다", this);
            }
        }

        private void LateUpdate()
        {
            if (currentRoom == null)
                return;
            if (!currentRoom.TryGetCameraBounds(out Bounds b))
                return;

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float x = (b.size.x <= halfW * 2) ? b.center.x
                                : Mathf.Clamp(target.position.x, b.min.x + halfW, b.max.x - halfW);
            float y = (b.size.y <= halfH * 2) ? b.center.y
                                : Mathf.Clamp(target.position.y, b.min.y + halfH, b.max.y - halfH);

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
