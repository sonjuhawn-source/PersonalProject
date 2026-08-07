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
        private string hitClip = "Hit";
        [SerializeField]
        private string deathClip = "Death";
        [SerializeField] 
        private float deathDelay = 0.35f;   //사망 클립 길이

        [SerializeField] private HurtBox hurtBox;

        private int currentHealth;

        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;

            if(hurtBox == null)
            {
                Debug.LogWarning($"{name}: Hurbox Null", this);
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
                animator.Play(hitClip);
        }

        private async UniTaskVoid Die()
        {
            Debug.Log($"{name} 사망", this);
            hurtBox.SetEnable(false);
            animator.Play(deathClip);
            await UniTask.Delay(TimeSpan.FromSeconds(deathDelay));
            Destroy(gameObject);
        }
    }
}
