using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Gameplay.Rooms
{
    public class Room : MonoBehaviour
    {
        [SerializeField]
        private Transform entry;
        [SerializeField]
        private Transform exit;
        [SerializeField]
        private Transform[] spawnPoints;
        [SerializeField]
        private Tilemap boundsSource;

        private Bounds localBounds;

        internal Transform Entry => entry;
        internal Transform Exit => exit;
        internal Transform[] SpawnPoints => spawnPoints;
        internal Bounds CameraBounds
        {
            get
            {
                Vector3 min = boundsSource.transform.TransformPoint(localBounds.min);
                Vector3 max = boundsSource.transform.TransformPoint(localBounds.max);

                Bounds b = new Bounds();
                b.SetMinMax(min, max);
                return b;
            }
        }


        private void Awake()
        {
            if (entry == null)
                Debug.LogWarning($"{gameObject.name}: entry 미지정 — 방을 이을 위치를 못 찾는다", this);
            if (exit == null)
                Debug.LogWarning($"{gameObject.name}: exit 미지정 — 다음 방으로 넘어갈 수 없다", this);

            if (boundsSource == null)
                Debug.LogWarning($"{gameObject.name}: boundsSource 미지정 — 카메라가 방 경계를 몰라 추적이 안 된다", this);
            else
            {
                boundsSource.CompressBounds();
                localBounds = boundsSource.localBounds;
            }
        }
    }
}
