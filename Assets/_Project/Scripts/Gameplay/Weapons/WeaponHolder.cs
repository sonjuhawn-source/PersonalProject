using UnityEngine;

namespace Game.Gameplay
{
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField]
        private WeaponData startingWeapon;
        [SerializeField]
        Animator animator;

        private WeaponInstance current;

        internal WeaponInstance Current => current;

        private void Awake()
        {
            if(startingWeapon == null)
            {
                Debug.LogWarning($"{gameObject.name}: StartingWeapon Null", this);
                return;
            }
            Equip(new WeaponInstance(startingWeapon));
        }

        internal void Equip(WeaponInstance weaponInstance)
        {
            current = weaponInstance;
            animator.runtimeAnimatorController = weaponInstance.Data.OverrideController;
        }
    }
}
