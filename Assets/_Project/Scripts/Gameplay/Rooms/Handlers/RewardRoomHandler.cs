using Game.Gameplay.Run;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Rooms
{
    internal class RewardRoomHandler : IRoomHandler
    {
        private const int UpgradeLevelsPerReward = 1;

        private RewardOption[] options;
        private Health playerHealth;
        private WeaponHolder playerWeapons;
        private WeaponData offeredWeapon;
        private int offeredHeal;
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
            playerWeapons = found != null ? found.GetComponent<WeaponHolder>() : null;
            offeredWeapon = PickUnheldWeapon(data, run);

            if (playerHealth == null)
            {
                Debug.LogWarning($"{room.name}: 플레이어 Health 가 없다 — 보상을 줄 대상이 없어 빈 방으로 통과한다", room);
                Resolve();
                return;
            }

            // 치명적이지 않다. 무기·강화만 빠지고 회복은 그대로 준다.
            if (playerWeapons == null)
                Debug.LogWarning($"{room.name}: 플레이어 WeaponHolder 가 없다 — 무기와 강화 선택지가 안 나온다", room);

            offeredHeal = data.HealAmount;

            options = Compose();
            
            if (options.Length == 0)
            {
                Debug.LogWarning($"{room.name}: 뽑을 보상이 없다 — 회복도 무기도 강화도 줄 게 없다", room);
                Resolve();
                return;
            }

            if (playerWeapons != null)
                playerWeapons.FreeSwap = true;

            Offered?.Invoke(options);
        }

        internal void Refresh()
        {
            if (resolved || playerHealth == null)
                return;

            options = Compose();
            if (options.Length == 0)
                return;

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
                    ApplyWeapon(option);
                    break;

                case RewardKind.Upgrade:
                    ApplyUpgrade(option);
                    break;
            }
        }

        private void ApplyWeapon(RewardOption option)
        {
            if (playerWeapons == null || option.Weapon == null)
            {
                Debug.LogWarning("보상 — 무기를 줄 대상이 없다");
                return;
            }

            if (!playerWeapons.Replace(option.TargetSlot, option.Weapon, option.Amount))
            {
                Debug.LogWarning($"보상 — {option.Weapon.name} 을 슬롯 {option.TargetSlot} 에 넣지 못했다");
                return;
            }

            Debug.Log($"보상 — 무기 {option.Weapon.name} +{option.Amount} 획득" +
                      $" · {(option.Dropped != null ? option.Dropped.name : "빈 칸")} 버림");
        }

        private void ApplyUpgrade(RewardOption option)
        {
            if (playerWeapons == null || !playerWeapons.UpgradeActive(option.Amount))
            {
                Debug.LogWarning("보상 — 강화할 무기가 없다");
                return;
            }

            // 화면에 뜬 대상(option.Weapon)을 찍는다. 지금 활성을 다시 읽으면
            // 둘이 갈릴 수 있고, 그러면 로그가 화면을 검증해주지 못한다.
            Debug.Log($"보상 — 강화 · {(option.Weapon != null ? option.Weapon.name : "빈 칸")}" +
                      $" +{playerWeapons.ActiveUpgradeLevel}");
        }

        private WeaponData PickUnheldWeapon(RoomData data, RunState run)
        {
            int count = data.RewardWeaponCount;
            if (count == 0)
                return null;

            var pool = new List<WeaponData>(count);
            for (int i = 0; i < count; i++)
            {
                WeaponData w = data.GetRewardWeapon(i);
                if (w == null || Holds(w))
                    continue;
                pool.Add(w);
            }

            if (pool.Count == 0)
                return null;

            return pool[run.NextInt(0, pool.Count)];
        }

        private RewardOption[] Compose()
        {
            var list = new List<RewardOption>(3);

            if (offeredWeapon != null && playerWeapons != null)
            {
                int slot = playerWeapons.InactiveIndex;

                int inherited = playerWeapons.ActiveUpgradeLevel;
                WeaponData dropped = playerWeapons.GetSlot(slot);
                list.Add(RewardOption.OfWeapon(offeredWeapon, slot, inherited, dropped));
            }

            if (offeredHeal > 0 && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                list.Add(RewardOption.OfHeal(offeredHeal));

            if (playerWeapons != null && playerWeapons.SlotCount > 0)
                list.Add(RewardOption.OfUpgrade(UpgradeLevelsPerReward,
                                                playerWeapons.GetSlot(playerWeapons.ActiveIndex)));

            return list.ToArray();
        }

        private bool Holds(WeaponData weapon)
        {
            if (playerWeapons == null)
                return false;

            for (int i = 0; i < playerWeapons.SlotCount; i++)
            {
                if (playerWeapons.GetSlot(i) == weapon)
                    return true;
            }
            return false;
        }

        private void Resolve()
        {
            resolved = true;
            if (playerWeapons != null)
                playerWeapons.FreeSwap = false;
            Cleared?.Invoke();
        }

        public void Exit()
        {
            if (playerWeapons != null)
                playerWeapons.FreeSwap = false;

            Offered = null;
            options = null;
            offeredWeapon = null;
            playerHealth = null;
            playerWeapons = null;
        }
    }
}
