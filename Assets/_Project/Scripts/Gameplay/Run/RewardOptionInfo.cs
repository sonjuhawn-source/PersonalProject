using UnityEngine.Localization;

namespace Game.Gameplay.Run
{
    public readonly struct RewardOptionInfo
    {
        public readonly RewardKind Kind;
        public readonly LocalizedString WeaponName;
        public readonly LocalizedString DroppedName;
        public readonly int Amount;

        internal RewardOptionInfo(RewardKind kind, LocalizedString weaponName, LocalizedString droppedName, int amount)
        {
            Kind = kind;
            WeaponName = weaponName;
            DroppedName = droppedName;
            Amount = amount;
        }
    }
}
