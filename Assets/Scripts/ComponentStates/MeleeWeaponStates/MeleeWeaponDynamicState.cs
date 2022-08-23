using UnityEngine;

namespace ComponentStates.MeleeWeaponStates {
    public class MeleeWeaponDynamicState : MeleeWeaponBaseState{
        public override void EnterState(MeleeWeaponStateManager meleeWeapon) {
            // meleeWeapon.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        }

        public override void UpdateState(MeleeWeaponStateManager meleeWeapon) {

        }

        public override void OnTriggerEnter2D(MeleeWeaponStateManager meleeWeapon, Collider2D other) {
        }
    }
}