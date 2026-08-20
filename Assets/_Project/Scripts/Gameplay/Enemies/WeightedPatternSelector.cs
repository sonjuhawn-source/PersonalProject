using UnityEngine;

namespace Game.Gameplay.Enemies
{
    internal class WeightedPatternSelector : IPatternSelector
    {
        private readonly AttackPattern[] patterns;
        private readonly float[] cooldowns;

        internal WeightedPatternSelector(AttackPattern[] patterns)
        {
            this.patterns = patterns;
            cooldowns = new float[patterns.Length];
        }

        public AttackPattern Select(float distance)
        {
            float total = 0f;
            int last = -1;

            for (int i = 0; i < patterns.Length; i++)
            {
                if (IsAvailable(i, distance) == false)
                    continue;
                total += patterns[i].Weight;
                last = i;
            }

            if (last < 0 || total <= 0f)
                return null;

            float r = Random.Range(0f, total);
            float acc = 0f;

            for (int i = 0; i < patterns.Length; i++)
            {
                if (IsAvailable(i, distance) == false)
                    continue;
                acc += patterns[i].Weight;
                if (r < acc)
                    return Use(i);
            }

            return Use(last);
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < cooldowns.Length; i++)
            {
                if (cooldowns[i] > 0f)
                    cooldowns[i] -= deltaTime;
            }
        }

        private bool IsAvailable(int i, float distance)
        {
            var p = patterns[i];
            if (p == null)
                return false;
            if (cooldowns[i] > 0f)
                return false;
            return distance >= p.MinRange && distance <= p.MaxRange;
        }

        private AttackPattern Use(int i)
        {
            cooldowns[i] = patterns[i].Cooldown;
            return patterns[i];
        }
    }
}