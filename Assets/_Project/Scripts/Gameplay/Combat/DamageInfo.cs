using UnityEngine;

namespace Game.Gameplay
{
    public class DamageInfo
    {
        public int Damage { get; private set; }
        public Vector2 Knockback { get; private set; }
        public float HitStopTime { get; private set; }
        public GameObject Source { get; private set; }

        public DamageInfo(int damage, Vector2 knockback, float hitStopTime, GameObject source)
        {
            Damage = damage;
            Knockback = knockback;
            HitStopTime = hitStopTime;
            Source = source;
        }
    }
}
