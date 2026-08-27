using UnityEngine;

namespace Game.Gameplay.Enemies
{
    [CreateAssetMenu(menuName = "Enemy/Attack Pattern")]
    public class AttackPattern : ScriptableObject
    {
        [Header("내용물")]
        [SerializeField]
        private AttackData attack;
        [SerializeField]
        private AttackKind kind;
        [SerializeField]
        private Projectile projectilePrefab;
        [SerializeField]
        private float telegraphTime;
        [SerializeField]
        private string telegraphStateName;
        [SerializeField]
        private string attackStateName;
        [SerializeField]
        private Vector2 hitboxOffset;
        [SerializeField]
        private Vector2 hitboxSize;
        [SerializeField]
        private float forwardSpeed;
        [SerializeField] 
        private bool interruptible = true;

        [Header("선택 조건")]
        [SerializeField]
        private float minRange = 0;
        [SerializeField]
        private float maxRange = 100;
        [SerializeField]
        private float maxHeightDiff = 100;
        [SerializeField]
        private float weight = 1;
        [SerializeField]
        private float cooldown;

        internal AttackData Attack => attack;
        internal AttackKind Kind => kind;
        internal Projectile ProjectilePrefab => projectilePrefab;
        internal float TelegraphTime => telegraphTime;
        internal string TelegraphStateName => telegraphStateName;
        internal string AttackStateName => attackStateName;
        internal Vector2 HitboxOffset => hitboxOffset;
        internal Vector2 HitboxSize => hitboxSize;
        internal float ForwardSpeed => forwardSpeed;
        internal float MaxRange => maxRange;
        internal float MinRange => minRange;
        internal float MaxHeightDiff => maxHeightDiff;
        internal float Weight => weight;
        internal float Cooldown => cooldown;
        internal bool Interruptible => interruptible;

    }
}
