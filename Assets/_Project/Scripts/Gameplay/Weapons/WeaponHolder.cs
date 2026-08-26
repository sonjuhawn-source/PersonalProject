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
