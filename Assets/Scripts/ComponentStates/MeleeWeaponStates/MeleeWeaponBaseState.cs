using UnityEngine;

namespace ComponentStates.MeleeWeaponStates {
    public abstract class MeleeWeaponBaseState {
        public abstract void EnterState(MeleeWeaponStateManager meleeWeapon);
        public abstract void UpdateState(MeleeWeaponStateManager meleeWeapon);
        public abstract void OnTriggerEnter2D(MeleeWeaponStateManager meleeWeapon, Collider2D other);
    }
}