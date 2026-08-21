using UnityEngine;

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
        internal Transform Entry => entry;
        internal Transform Exit => exit;
        internal Transform[] SpawnPoints => spawnPoints;

        private void Awake()
        {
            if (entry == null)
                Debug.LogWarning($"{name}: entry 미지정 — 방을 이을 위치를 못 찾는다", this);
            if (exit == null)
                Debug.LogWarning($"{name}: exit 미지정 — 다음 방으로 넘어갈 수 없다", this);
        }
    }
}
