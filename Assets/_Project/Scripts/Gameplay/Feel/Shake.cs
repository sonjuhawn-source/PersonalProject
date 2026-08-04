using UnityEngine;

namespace Game.Gameplay
{
    public class Shake: MonoBehaviour
    {
        [SerializeField]
        private float duration = 0.12f;

        private static Shake instance;
        private Transform tr;
        private float amplitude;
        private float remaining;

        private void Awake()
        {
            if(instance == null)
                instance = this;

            tr = transform;
        }

        private void OnDestroy()
        {
            if(instance == this)
                instance = null;
        }

        internal static void Play(float strength)
        {
            if (instance == null)
                return;
            instance.Begin(strength);
        }

        private void Begin(float strength)
        {
            if (strength <= 0)
                return;

            amplitude = Mathf.Max(amplitude, strength);
            remaining = Mathf.Max(remaining, duration);
        }

        private void LateUpdate()
        {
            if (remaining <= 0)
                return;

            float t = remaining / duration;
            remaining -= Time.unscaledDeltaTime;

            if (remaining <= 0)                  // 이번 프레임에 끝났다
            {
                amplitude = 0;
                tr.localPosition = Vector3.zero;
                return;
            }
            float a = amplitude * t;
            tr.localPosition = new Vector3(Random.Range(-a, a), Random.Range(-a, a), 0f);
        }
    }
}
