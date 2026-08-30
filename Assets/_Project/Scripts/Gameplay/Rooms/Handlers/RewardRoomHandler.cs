using Game.Gameplay.Run;
using System;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    internal class RewardRoomHandler : IRoomHandler
    {
        private const int OptionCount = 3;

        private RewardOption[] options;
        private Health playerHealth;
        private bool resolved;

        public event Action Cleared;
        public bool EndsRun => false;

        // StageRunner 가 받아서 public 면으로 다시 쏜다.
        // Game.UI 는 이 핸들러를 못 본다 — internal 이고 asmdef 가 단방향이다.
        internal event Action<RewardOption[]> Offered;

        public void Enter(Room room, RoomData data, RunState run)
        {
            resolved = false;

            GameObject found = GameObject.FindWithTag("Player");
            playerHealth = found != null ? found.GetComponent<Health>() : null;

            if (playerHealth == null)
            {
                Debug.LogWarning($"{room.name}: 플레이어 Health 가 없다 — 보상을 줄 대상이 없어 빈 방으로 통과한다", room);
                Resolve();
                return;
            }

            options = Build(data, run);

            if (options.Length == 0)
            {
                Debug.LogWarning($"{room.name}: 뽑을 보상이 없다 — healAmount 와 rewardWeapons 가 둘 다 비었다", room);
                Resolve();
                return;
            }

            Offered?.Invoke(options);
        }

        // 선택이 오기 전까지 Cleared 를 쏘지 않는다. 출구가 안 열린 채로 대기한다.
        internal void Choose(int index)
        {
            if (resolved)
                return;
            if (options == null || index < 0 || index >= options.Length)
                return;

            Apply(options[index]);
            Resolve();
        }

        // 종류를 늘릴 때 고치는 곳은 여기 하나다.
        private void Apply(RewardOption option)
        {
            switch (option.Kind)
            {
                case RewardKind.Heal:
                    int before = playerHealth.CurrentHealth;
                    playerHealth.Heal(option.Amount);
                    Debug.Log($"보상 — 회복 · HP {before} → {playerHealth.CurrentHealth}");
                    break;

                case RewardKind.Weapon:
                    // B 에서 채운다 — 슬롯 교체가 필요하다.
                    Debug.Log($"보상 — 무기 {option.Weapon?.name} (아직 적용 안 됨)");
                    break;
            }
        }

        private RewardOption[] Build(RoomData data, RunState run)
        {
            bool canHeal = data.HealAmount > 0;
            int weaponCount = data.RewardWeaponCount;

            if (!canHeal && weaponCount == 0)
                return Array.Empty<RewardOption>();

            var picked = new RewardOption[OptionCount];
            int healCount = 0;

            for (int i = 0; i < OptionCount; i++)
            {
                bool takeHeal = canHeal && (weaponCount == 0 || run.NextInt(0, 2) == 0);

                if (takeHeal)
                {
                    picked[i] = RewardOption.OfHeal(data.HealAmount);
                    healCount += 1;
                }
                else
                {
                    picked[i] = RewardOption.OfWeapon(data.GetRewardWeapon(run.NextInt(0, weaponCount)));
                }
            }

            // 회복은 포기가 없다 (GDD 6장). 셋 다 회복이면 고르는 행위가 사라진다.
            if (healCount == OptionCount && weaponCount > 0)
                picked[0] = RewardOption.OfWeapon(data.GetRewardWeapon(run.NextInt(0, weaponCount)));

            return picked;
        }

        private void Resolve()
        {
            resolved = true;
            Cleared?.Invoke();
        }

        public void Exit()
        {
            Offered = null;
            options = null;
            playerHealth = null;
        }
    }
}
