using System;
using UnityEngine;

namespace Game.Gameplay
{
    [Serializable]
    public struct AttackData
    {
        public float startup;
        public float activeTime;
        public float recovery;
        public int damage;
        public float knockbackForce;
        public float hitStopTime;
    }
}
