namespace Game.Gameplay.Run
{
    internal enum RewardKind
    {
        Weapon,
        Heal,
    }

    // 뽑힌 선택지 하나. 뽑은 뒤에는 안 바뀌므로 읽기 전용이다.
    // 종류를 늘릴 때 고칠 곳은 이 enum 과 RewardRoomHandler.Apply 의 switch 뿐이다.
    internal readonly struct RewardOption
    {
        internal readonly RewardKind Kind;
        internal readonly WeaponData Weapon;   // Kind == Weapon
        internal readonly int Amount;          // Kind == Heal

        private RewardOption(RewardKind kind, WeaponData weapon, int amount)
        {
            Kind = kind;
            Weapon = weapon;
            Amount = amount;
        }

        internal static RewardOption OfWeapon(WeaponData weapon)
            => new RewardOption(RewardKind.Weapon, weapon, 0);

        internal static RewardOption OfHeal(int amount)
            => new RewardOption(RewardKind.Heal, null, amount);

        // UI 가 없는 동안 로그로 읽는다. #103 이 LocalizedString 으로 바꾼다.
        internal string Describe()
        {
            switch (Kind)
            {
                case RewardKind.Weapon: return $"무기 — {(Weapon != null ? Weapon.name : "빈 칸")}";
                case RewardKind.Heal: return $"회복 — HP +{Amount}";
                default: return "알 수 없는 보상";
            }
        }
    }
}
