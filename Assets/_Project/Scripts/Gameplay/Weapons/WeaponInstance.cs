using UnityEngine;

namespace Game.Gameplay
{
    public class WeaponInstance
    {
        internal readonly WeaponData Data;

        // 배율이 아니라 레벨을 저장한다.
        // 교체 인계가 "-1" 이라 셀 수 있어야 하고, 배율을 저장하면
        // 1.3 에서 한 단계 빼는 값이 얼마인지를 매번 되짚어야 한다.
        internal int UpgradeLevel { get; private set; }

        private const float DamagePerLevel = 0.15f;
        private const float SpeedPerLevel = 0.07f;

        private float swapCooldownRemaining;

        internal float SwapReadyRatio
        {
            get
            {
                if (Data.SwapCooldown <= 0)
                    return 1f;

                return Mathf.Clamp01(1f - swapCooldownRemaining / Data.SwapCooldown);
            }
        }

        internal WeaponInstance(WeaponData data, int upgradeLevel = 0)
        {
            Data = data;
            UpgradeLevel = Mathf.Max(0, upgradeLevel);
        }

        internal void Upgrade(int levels)
        {
            if (levels <= 0)
                return;
            UpgradeLevel += levels;
        }

        internal int ComboCount => Data.ComboCount;

        // WeaponData 는 ScriptableObject 다. 원본을 고치면 에셋 파일이 더러워져
        // 재시작 후에도 남고, 두 슬롯이 같은 무기일 때 서로 샌다.
        // AttackData 가 struct 라 GetAttack 이 사본을 주므로 여기서 고쳐도 원본은 안 변한다.
        internal AttackData GetAttack(int i)
        {
            AttackData a = Data.GetAttack(i);
            if (UpgradeLevel <= 0)
                return a;

            // 반올림이 먹어버리면 "강화했는데 그대로" 가 된다. 레벨당 최소 1 은 보장한다.
            float scaled = a.damage * (1f + UpgradeLevel * DamagePerLevel);
            a.damage = Mathf.Max(a.damage + UpgradeLevel, Mathf.RoundToInt(scaled));

            // activeTime 은 건드리지 않는다 — 히트박스가 켜져 있는 시간이라
            // 줄이면 공속이 오를수록 오히려 안 맞는다.
            float speed = 1f + UpgradeLevel * SpeedPerLevel;
            a.startup /= speed;
            a.recovery /= speed;

            return a;
        }

        internal bool IsSwapReady => swapCooldownRemaining <= 0;

        internal void Tick(float dt)
        {
            if (swapCooldownRemaining > 0)
                swapCooldownRemaining -= dt;
        }

        internal void StartSwapCooldown()
        {
            swapCooldownRemaining = Data.SwapCooldown;
        }
    }
}
