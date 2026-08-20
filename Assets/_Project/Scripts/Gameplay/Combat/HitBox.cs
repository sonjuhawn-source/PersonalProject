using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class HitBox : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D hitBoxCollider;

        private HashSet<GameObject> target = new HashSet<GameObject>();
        private DamageInfo damageInfo;
        private bool active;

        public event Action<HurtBox> Hit;

        private void Awake()
        {
            if (hitBoxCollider == null)
                hitBoxCollider = GetComponent<BoxCollider2D>();
            hitBoxCollider.enabled = false;
        }
        public void HitBoxActivate(DamageInfo info)
        {
            active = true;
            damageInfo = info;
            target.Clear();
            hitBoxCollider.enabled = true;
        }

        public void HitBoxDeactivate()
        {
            active = false;
            hitBoxCollider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (active == false)
                return;

            if (collision.TryGetComponent(out HurtBox hurtbox) == false)
                return;

            if (hurtbox.Owner == damageInfo.Source)
                return;

            if (target.Contains(hurtbox.Owner))
                return;

            target.Add(hurtbox.Owner);
            if (hurtbox.TakeHit(damageInfo) == false)
                return;
            HitStop.Play(damageInfo.HitStopTime);
            Shake.Play(damageInfo.ShakeStrength);
            Hit?.Invoke(hurtbox);
        }

        internal void SetShape(Vector2 offset, Vector2 size)
        {
            hitBoxCollider.offset = offset;
            hitBoxCollider.size = size;
        }
    }
}
