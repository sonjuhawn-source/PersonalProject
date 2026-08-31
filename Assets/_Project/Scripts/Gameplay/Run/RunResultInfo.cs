using UnityEngine.Localization;

namespace Game.Gameplay.Run
{
    public readonly struct RunWeaponInfo
    {
        public readonly LocalizedString Name;
        public readonly int Level;
        internal RunWeaponInfo(LocalizedString name, int level)
        {
            Name = name;
            Level = level;
        }
    }

    public readonly struct RunResultInfo 
    {
        public readonly RunOutcome Outcome;
        public readonly int Floor;
        public readonly int Kills;
        public readonly int Health;
        public readonly RunWeaponInfo[] Weapons;
        internal RunResultInfo(RunOutcome outcome, int floor, int kills,  int health, RunWeaponInfo[] weapons)
        {
            Outcome = outcome;
            Floor = floor;
            Kills = kills;
            Health = health;
            Weapons = weapons;
        }
    }
}
