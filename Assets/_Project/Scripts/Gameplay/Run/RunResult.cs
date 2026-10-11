namespace Game.Gameplay.Run
{
    public enum RunOutcome
    {
        Cleared,
        Died,
    }

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
