using UnityEngine;

namespace Game.Gameplay
{
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField]
        private WeaponData[] startingWeapons;
        [SerializeField]
        Animator animator;
        [SerializeField]
        private HitBox hitBox;

        private WeaponInstance[] slots;
        private int activeIndex;

        internal WeaponInstance Current => slots[activeIndex];

        internal int SlotCount => slots == null ? 0 : slots.Length;
        internal int ActiveIndex => activeIndex;

        // 보상은 안 쓰던 무기를 버린다. 지키고 싶으면 미리 스왑해서 활성으로 두면 된다.
        internal int InactiveIndex => slots == null ? -1 : (activeIndex + 1) % slots.Length;
        internal int ActiveLevel => slots == null ? 0 : slots[activeIndex].UpgradeLevel;

        internal WeaponData GetSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length)
                return null;
            return slots[index].Data;
        }

        // 새 무기는 인계받은 레벨로 들어온다. 0 으로 들어오면 후반에 무기 보상이 죽는다.
        internal bool Replace(int index, WeaponData data, int upgradeLevel)
        {
            if (slots == null || data == null || index < 0 || index >= slots.Length)
                return false;

            slots[index] = new WeaponInstance(data, upgradeLevel);

            // 지금 보상 경로는 비활성 슬롯만 바꾸므로 안 탄다.
            // #98 적 무기 드랍이 활성 슬롯을 바꿀 때 이게 없으면 외형·모션·리치가 옛 무기로 남는다.
            if (index == activeIndex)
                Equip(activeIndex);

            return true;
        }

        internal bool UpgradeActive(int levels)
        {
            if (slots == null)
                return false;

            slots[activeIndex].Upgrade(levels);
            return true;
        }

        internal WeaponData[] SnapshotWeapons()
        {
            if (slots == null)
                return System.Array.Empty<WeaponData>();

            var result = new WeaponData[slots.Length];
            for (int i = 0; i < slots.Length; i++)
                result[i] = slots[i].Data;
            return result;
        }

        private void Awake()
        {
            if (startingWeapons == null || startingWeapons.Length < 2)
            {
                Debug.LogWarning($"{gameObject.name}: startingWeapons 가 2개 미만이다 — 무기가 초기화되지 않아 공격하면 터진다", this);
                return;
            }

            for (int i = 0; i < startingWeapons.Length; i++)
            {
                if (startingWeapons[i] == null)
                {
                    Debug.LogWarning($"{gameObject.name}: startingWeapons[{i}] 가 비어 있다 — 무기가 초기화되지 않아 공격하면 터진다", this);
                    return;
                }
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    Debug.LogWarning($"{gameObject.name}: animator 미지정 — 자식에서 찾았다. 인스펙터 배선을 확인해라", this);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: animator 를 자식에서도 못 찾았다. 무기를 바꿔도 외형과 모션이 안 바뀐다", this);
                }
            }

            if (hitBox == null)
            {
                hitBox = GetComponentInChildren<HitBox>();
                if (hitBox != null)
                {
                    Debug.LogWarning($"{gameObject.name}: hitBox 미지정 — 자식에서 찾았다. 인스펙터 배선을 확인해라", this);
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: hitBox 를 자식에서도 못 찾았다. 무기 사거리가 적용되지 않고 공격 시 터진다", this);
                }
            }

            slots = new WeaponInstance[startingWeapons.Length];
            for (int i = 0; i < startingWeapons.Length; i++)
            {
                slots[i] = new WeaponInstance(startingWeapons[i]);
            }

            activeIndex = 0;
            Equip(activeIndex);
        }

        private void Update()
        {
            if (slots == null)
                return;

            foreach (var s in slots)
                s.Tick(Time.deltaTime);
        }

        internal bool TrySwap()
        {
            if (slots == null)
                return false;

            var next = (activeIndex + 1) % slots.Length;
            if (!slots[next].IsSwapReady)
                return false;

            slots[activeIndex].StartSwapCooldown();

            activeIndex = next;
            Equip(activeIndex);
            return true;
        }

        private void Equip(int index)
        {
            var data = slots[index].Data;

            if(animator != null)
            {
                animator.runtimeAnimatorController = data.OverrideController;

                if (!animator.HasState(0, Animator.StringToHash(data.AttackStateName)))
                    Debug.LogWarning($"{data.name}: 공격 상태 '{data.AttackStateName}' 이 컨트롤러에 없다", this);
            }

            if (data.Kind == AttackKind.Projectile && data.ProjectilePrefab == null)
                Debug.LogWarning($"{data.name}: 원거리 무기인데 투사체 프리팹이 없다. 공격하면 AttackState 에 갇힌다", this);

            hitBox?.SetShape(data.HitboxOffset, data.HitboxSize);
        }
    }
}
