using UnityEngine;

namespace Game.Gameplay
{
    public class Health : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth =30;

        private int currentHealth;

        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0)
                return;

            currentHealth -= amount;

            if(currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("사망");
        }
    }
}
