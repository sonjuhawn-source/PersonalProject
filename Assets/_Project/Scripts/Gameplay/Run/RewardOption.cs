
namespace Game.Gameplay.Run
{
    public enum RewardKind
    {
        Weapon,
        Heal,
        Upgrade,
    }

    // 뽑힌 선택지 하나. 뽑은 뒤에는 안 바뀌므로 읽기 전용이다.
    // 종류를 늘릴 때 고칠 곳은 이 enum 과 RewardRoomHandler.Apply 의 switch 뿐이다.
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

        // 진단용 로그. 화면이 안 뜰 때 뽑기 문제인지 배선 문제인지 가른다.
        // ToInfo() 와 달리 로컬라이즈를 안 타므로 테이블이 깨져도 이건 읽힌다.
        // 화면과 같은 말을 해야 쓸모가 있어서 인계 레벨과 버릴 무기를 같이 찍는다.
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
