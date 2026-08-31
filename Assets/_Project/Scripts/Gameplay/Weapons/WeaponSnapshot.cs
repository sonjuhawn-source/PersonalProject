using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct WeaponSnapshot
    {
        internal readonly WeaponData Data;
        internal readonly int Level;

        internal WeaponSnapshot(WeaponData data, int level)
        {
            Data = data;
            Level = level;
        }
    }
}
