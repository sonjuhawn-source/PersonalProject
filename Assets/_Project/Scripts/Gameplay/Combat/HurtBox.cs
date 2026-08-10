using UnityEngine;

namespace Game.Gameplay
{
    public class HurtBox : MonoBehaviour
    {
        [SerializeField]
        GameObject owner;
        [SerializeField]
        Knockback knockback;
        [SerializeField]
        Health health;
        [SerializeField]
        Collider2D hurtBoxCollider;
        [SerializeField]
        Invincibility invincibility;


        public GameObject Owner => owner;

        private void Awake()
        {
            if (owner == null)
            {
                owner = transform.parent != null ? transform.parent.gameObject : gameObject;
                Debug.LogWarning($"{name}: owner 미지정 — {owner.name} 로 폴백했다. 중복 히트 방지가 어긋날 수 있다.", this);
            }
        }

        public bool TakeHit(DamageInfo info)
        {
            if (invincibility != null && invincibility.IsActive)
                return false;

            Debug.Log($"{Owner.name} 이(가) {info.Damage}맞음, 출처 {info.Source.name}");
            knockback?.Apply(info.Knockback);
            health?.TakeDamage(info.Damage);
            invincibility?.Begin();
            return true;
        }

        public void SetEnable(bool value) => hurtBoxCollider.enabled = value;
    }
}
