using UnityEngine;
using UnityEngine.Localization;

namespace Game.Gameplay
{
    [CreateAssetMenu(menuName = "Weapon/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
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
        internal AnimatorOverrideController OverrideController => overrideController;
        internal LocalizedString DisplayName  => displayName;
    }
}
