using UnityEngine;
using UnityEngine.Localization;

namespace Game.Gameplay
{
    internal enum AttackKind 
    {
        Melee,
        Projectile
    }
    [CreateAssetMenu(menuName = "Weapon/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField]
        private AttackKind kind;
        [SerializeField]
        private Projectile projectilePrefab;

        [SerializeField]
        private string attackStateName;

        [SerializeField]
        private LocalizedString displayName;
        [SerializeField]
        private LocalizedString description;

        [SerializeField]
        AnimatorOverrideController overrideController;

        [SerializeField]
        private AttackData[] combo;
        [SerializeField]
        private float clipWindup;
        [SerializeField]
        private float forwardSpeed;
        [SerializeField]
        private float swapCooldown;

        [SerializeField]
        private int rarity;

        internal int ComboCount => combo.Length;
        internal AttackData GetAttack(int i) => combo[i];
        internal float ClipWindup => clipWindup;
        internal float ForwardSpeed => forwardSpeed;
        internal float SwapCooldown => swapCooldown;
        internal string AttackStateName => attackStateName;
        internal AnimatorOverrideController OverrideController => overrideController;
        internal LocalizedString DisplayName  => displayName;
        internal AttackKind Kind => kind;
        internal Projectile ProjectilePrefab => projectilePrefab;
    }
}
