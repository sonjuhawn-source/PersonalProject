namespace Game.Gameplay.Run
{
    public enum RunOutcome
    {
        Cleared,
        Died,
    }

    // 런이 끝난 시점의 값만 담는다. 끝난 뒤에는 안 바뀌므로 읽기 전용이다.
    internal readonly struct RunResult
    {
        internal readonly RunOutcome Outcome;
        internal readonly int Floor;
        internal readonly int Kills;
        internal readonly int Health;
        internal readonly WeaponSnapshot[] Weapons;

        internal RunResult(RunOutcome outcome, int floor, int kills, int health, WeaponSnapshot[] weapons)
        {
            Outcome = outcome;
            Floor = floor;
            Kills = kills;
            Health = health;
            Weapons = weapons;
        }

        internal RunResultInfo ToInfo()
        {
            RunWeaponInfo[] infos = new RunWeaponInfo[Weapons.Length];
            for(int i = 0; i < Weapons.Length; i++)
            {
                WeaponData data = Weapons[i].Data;
                var name = data != null ? data.DisplayName : null;
                infos[i] = new RunWeaponInfo(name, Weapons[i].Level);
            }

            return new RunResultInfo(Outcome, Floor, Kills, Health, infos);
        }
    }
}
