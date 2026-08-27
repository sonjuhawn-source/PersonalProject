using System;
using UnityEngine;

namespace Game.Gameplay
{
    public class Health : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth =30;

        [SerializeField] private HurtBox hurtBox;

        private int currentHealth;

        public event Action Died;
        public event Action Damaged;

        public int MaxHealth => maxHealth;
        public event Action Changed;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;

            if(hurtBox == null)
            {
                Debug.LogWarning($"{name}: hurtBox 미지정 — 죽어도 허트박스가 안 꺼진다. 시체가 계속 맞는다", this);
            }
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0)
                return;

            SetHealth(currentHealth - amount);   
            
            if (currentHealth <= 0)
                Die();
            else
                Damaged?.Invoke();
        }

        public void Heal(int amount)
        {
            if(currentHealth <= 0) 
                return;
            SetHealth(currentHealth + amount);
        }

        // 음수 체력은 어디서도 의미가 없다. 소비자마다 가리면 다음 소비자가 또 물린다.
        private void SetHealth(int value)
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            Changed?.Invoke();
        }

        private void Die()
        {
            Debug.Log($"{name} 사망", this);
            hurtBox?.SetEnable(false);
            Died?.Invoke();
        }
    }
}
