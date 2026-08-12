using UnityEngine;

namespace Game.Gameplay
{
    public class HurtBox : MonoBehaviour
    {
        [SerializeField]
        GameObject owner;
        [SerializeField]
        VelocityImpulse impulse;
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
                Debug.LogWarning($"{name}: owner 미지정 — {owner.name} 로 폴백했다. " +
                    $"중복 히트 방지가 어긋날 수 있다.", this);
            }

            if (invincibility != null && invincibility.gameObject != owner)
            {
                Debug.LogWarning($"{owner.name} 의 HurtBox: invincibility 가 {invincibility.gameObject.name} 것이다. " +
                    $"무적이 공유돼 상대가 맞아도 내가 무적이 된다. " +
                    $"None 으로 비우거나 {owner.name} 의 컴포넌트를 꽂아라.", this);
            }

            if (impulse != null && impulse.gameObject != owner)
            {
                Debug.LogWarning($"{owner.name} 의 HurtBox: impulse 가 {impulse.gameObject.name} 것이다. " +
                    $"상대가 맞을 때 내가 밀린다. " +
                    $"None 으로 비우거나 {owner.name} 의 컴포넌트를 꽂아라.", this);
            }

            if (health != null && health.gameObject != owner)
            {
                Debug.LogWarning($"{owner.name} 의 HurtBox: health 가 {health.gameObject.name} 것이다. " +
                    $"상대가 맞을 때 내 체력이 깎인다. None 으로 비우거나 {owner.name} 의 컴포넌트를 꽂아라.", this);
            }
        }

        public bool TakeHit(DamageInfo info)
        {
            if (invincibility != null && invincibility.IsActive)
                return false;

            Debug.Log($"{Owner.name} 이(가) {info.Damage}맞음, 출처 {info.Source.name}");
            impulse?.Apply(info.Knockback);
            health?.TakeDamage(info.Damage);
            invincibility?.Begin();
            return true;
        }

        public void SetEnable(bool value) => hurtBoxCollider.enabled = value;
    }
}
