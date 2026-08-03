using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class HitBox : MonoBehaviour
    {
        [SerializeField]
        private Collider2D hitBoxCollider;

        private HashSet<GameObject> target = new HashSet<GameObject>();
        private DamageInfo damageInfo;

        private void Awake()
        {
            if (hitBoxCollider == null)
            {
                hitBoxCollider = GetComponent<Collider2D>();
            }
            hitBoxCollider.enabled = false;
        }
        public void HitBoxActivate(DamageInfo info)
        {
            damageInfo = info;
            target.Clear();
            hitBoxCollider.enabled = true;
        }

        public void HitBoxDeactivate()
        {
            hitBoxCollider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out HurtBox hurtbox) == false)
                return;

            if (hurtbox.Owner == damageInfo.Source)
                return;

            if (target.Contains(hurtbox.Owner))
                return;

            target.Add(hurtbox.Owner);
            hurtbox.TakeHit(damageInfo);
            HitStop.Play(damageInfo.HitStopTime).Forget();
        }
    }
}
