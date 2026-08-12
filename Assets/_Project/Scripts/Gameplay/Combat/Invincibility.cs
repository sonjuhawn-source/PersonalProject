using UnityEngine;

namespace Game.Gameplay
{
    public class Invincibility : MonoBehaviour
    {
        [SerializeField]
        private float duration = 0.6f;
        [SerializeField]
        private float blinkInterval = 0.08f;
        [SerializeField]
        private SpriteRenderer sprite;

        private float remaining;
        private bool blinking;

        internal bool IsActive => remaining > 0;

        private void Awake()
        {
            if (sprite == null)
            {
                Debug.LogWarning($"{gameObject.name} Sprite가 배선되지 않았습니다", this);
            }
        }

        private void Update()
        {
            if (remaining <= 0)
                return;

            remaining -= Time.deltaTime;

            if(remaining <= 0)
            {
                remaining = 0;
                blinking = false;
                sprite.enabled = true;
                return;
            }

            if(blinking)
                sprite.enabled = ((int)(remaining / blinkInterval)) % 2 == 0;
        }

        internal void Begin() => Begin(duration);
        internal void Begin(float time, bool blink = true)
        {
            remaining = Mathf.Max(remaining, time);
            blinking = blinking || blink;
        }
    }
}
