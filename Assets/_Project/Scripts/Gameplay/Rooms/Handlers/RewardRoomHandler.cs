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

            if (playerHealth == null)
            {
                Debug.LogWarning($"{room.name}: 플레이어 Health 가 없다 — 보상을 줄 대상이 없어 빈 방으로 통과한다", room);
                Resolve();
                return;
            }

            // 치명적이지 않다. 무기·강화만 빠지고 회복은 그대로 준다.
            if (playerWeapons == null)
                Debug.LogWarning($"{room.name}: 플레이어 WeaponHolder 가 없다 — 무기와 강화 선택지가 안 나온다", room);

            options = Build(data, run);

            if (options.Length == 0)
            {
                Debug.LogWarning($"{room.name}: 뽑을 보상이 없다 — 회복도 무기도 강화도 줄 게 없다", room);
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
                    ApplyWeapon(option.Weapon);
                    break;

                case RewardKind.Upgrade:
                    ApplyUpgrade(option.Amount);
                    break;
            }
        }

        private void ApplyWeapon(WeaponData weapon)
        {
            if (playerWeapons == null || weapon == null)
            {
                Debug.LogWarning("보상 — 무기를 줄 대상이 없다");
                return;
            }

            int slot = playerWeapons.InactiveIndex;

            // 인계 기준은 활성 슬롯이다. 버리는 쪽(비활성)은 강화를 받은 적이 없어
            // 거의 항상 0 이고, 그걸 기준으로 하면 규칙이 한 번도 안 돈다.
            int inherited = Mathf.Max(0, playerWeapons.ActiveLevel - 1);
            WeaponData dropped = playerWeapons.GetSlot(slot);

            if (!playerWeapons.Replace(slot, weapon, inherited))
            {
                Debug.LogWarning($"보상 — {weapon.name} 을 슬롯 {slot} 에 넣지 못했다");
                return;
            }

            // 무엇을 버렸는지가 이 선택의 값이다. #103 이 이 문구를 그대로 화면에 쓴다.
            Debug.Log($"보상 — 무기 {weapon.name} +{inherited} 획득 · {(dropped != null ? dropped.name : "빈 칸")} 버림");
        }

        private void ApplyUpgrade(int levels)
        {
            if (playerWeapons == null || !playerWeapons.UpgradeActive(levels))
            {
                Debug.LogWarning("보상 — 강화할 무기가 없다");
                return;
            }

            WeaponData active = playerWeapons.GetSlot(playerWeapons.ActiveIndex);
            Debug.Log($"보상 — 강화 · {(active != null ? active.name : "빈 칸")} +{playerWeapons.ActiveLevel}");
        }

        // 종류가 겹치지 않게 하나씩 뽑는다. 확률로 뽑으면 셋 다 회복이 나올 수 있고
        // 그러면 고르는 행위가 사라진다 (GDD 6장 — 회복은 포기가 없다).
        private RewardOption[] Build(RoomData data, RunState run)
        {
            var list = new List<RewardOption>(3);

            // 이미 든 무기를 빼면 중복 장착을 허용할지 정할 필요가 없어진다 (GDD 12.6).
            WeaponData weapon = PickUnheldWeapon(data, run);
            if (weapon != null)
                list.Add(RewardOption.OfWeapon(weapon));

            // 만피면 죽은 선택지다. 넣으면 3택이 사실상 2택이 된다.
            if (data.HealAmount > 0 && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                list.Add(RewardOption.OfHeal(data.HealAmount));

            if (playerWeapons != null && playerWeapons.SlotCount > 0)
                list.Add(RewardOption.OfUpgrade(UpgradeLevelsPerReward));

            return list.ToArray();
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
            Cleared?.Invoke();
        }

        public void Exit()
        {
            Offered = null;
            options = null;
            playerHealth = null;
            playerWeapons = null;
        }
    }
}
