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

        private void Awake()
        {
            if(startingWeapons == null || startingWeapons.Length <2)
            {
                Debug.LogWarning($"{gameObject.name}: Slots must be 2 slots", this);
                return;
            }

            for(int i = 0; i< startingWeapons.Length; i++)
            {
                if (startingWeapons[i] == null)
                {
                    Debug.LogWarning($"{gameObject.name}: Slot {i} is null");
                    return;
                }
            }

            if(hitBox == null)
            {
                hitBox = GetComponentInChildren<HitBox>();
                Debug.LogWarning($"{gameObject.name}: hitBox 미지정 — 자식에서 찾았다. 배선을 확인해라");
            }

            slots = new WeaponInstance[startingWeapons.Length];
            for(int i = 0;i< startingWeapons.Length; i++)
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

            foreach(var s in slots)
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
            animator.runtimeAnimatorController = slots[index].Data.OverrideController;

            var stateName = slots[index].Data.AttackStateName;
            if(!animator.HasState(0, Animator.StringToHash(stateName)))
            {
                Debug.LogWarning($"{slots[index].Data.name}: 공격 상태 '{stateName}' 이 컨트롤러에 없다", this);
            }
            hitBox.SetShape(slots[index].Data.HitboxOffset, slots[index].Data.HitboxSize);
        }
    }
}
