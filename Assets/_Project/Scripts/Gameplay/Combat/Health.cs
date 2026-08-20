using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Gameplay
{
    public class Health : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth =30;
        [SerializeField]
        private Animator animator;
        [SerializeField] 
        private string deathClip = "Death";
        [SerializeField] 
        private float deathDelay = 0.35f;   //사망 클립 길이

        [SerializeField] private HurtBox hurtBox;

        private int currentHealth;

        public event Action Died;
        public event Action Damaged;

        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;

            if(hurtBox == null)
            {
                Debug.LogWarning($"{name}: hurtBox 미지정 — 죽어도 허트박스가 안 꺼진다. 시체가 계속 맞는다", this);
            }
            if (animator == null)
            {
                Debug.LogWarning($"{name}: animator 미지정 — 피격·사망 모션이 안 나온다. 데미지 처리는 정상이다", this);
            }
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0)
                return;

            currentHealth -= amount;

            if (currentHealth <= 0)
                Die().Forget();
            else
                Damaged?.Invoke();
        }

        private async UniTaskVoid Die()
        {
            Debug.Log($"{name} 사망", this);
            hurtBox?.SetEnable(false);
            Died?.Invoke();
            animator?.Play(deathClip);
            await UniTask.Delay(TimeSpan.FromSeconds(deathDelay));
            Destroy(gameObject);
        }
    }
}
