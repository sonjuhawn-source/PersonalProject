
namespace Game.Gameplay.Run
{
    public enum RewardKind
    {
        Weapon,
        Heal,
        Upgrade,
    }

    // 종류를 늘릴 때 고칠 곳은 이 enum 과 RewardRoomHandler.Apply 뿐이다.
    internal readonly struct RewardOption
    {
        internal readonly RewardKind Kind;
        internal readonly WeaponData Weapon;   
        internal readonly int Amount;          
        internal readonly int TargetSlot;
        internal readonly WeaponData Dropped;
        private RewardOption(RewardKind kind, WeaponData weapon, int amount, int targetSlot, WeaponData dropped)
        {
            Kind = kind;
            Weapon = weapon;
            Amount = amount;
            TargetSlot = targetSlot;
            Dropped = dropped;
        }

        internal static RewardOption OfWeapon(WeaponData weapon, int targetSlot, int inheritedLevel, WeaponData dropped)
            => new RewardOption(RewardKind.Weapon, weapon, inheritedLevel, targetSlot, dropped);

        internal static RewardOption OfHeal(int amount)
            => new RewardOption(RewardKind.Heal, null, amount, -1, null);

        internal static RewardOption OfUpgrade(int levels, WeaponData target)
            => new RewardOption(RewardKind.Upgrade, target, levels, -1, null);

        // 진단용. 로컬라이즈를 안 타므로 테이블이 깨져도 읽힌다.
        internal string Describe()
        {
            switch (Kind)
            {
                case RewardKind.Weapon:
                    return $"무기 — {(Weapon != null ? Weapon.name : "빈 칸")} +{Amount}" +
                           $" · {(Dropped != null ? Dropped.name : "빈 칸")} 버림";
                case RewardKind.Heal: return $"회복 — HP +{Amount}";
                case RewardKind.Upgrade: return $"강화 — {Weapon?.name} +{Amount}";
                default: return "알 수 없는 보상";
            }
        }

        internal RewardOptionInfo ToInfo()
        {
            return new RewardOptionInfo(
                Kind,
                Weapon != null ? Weapon.DisplayName : null,
                Dropped != null ? Dropped.DisplayName : null,
                Amount);
        }
    }
}
