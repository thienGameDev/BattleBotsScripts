using UnityEngine;

namespace ComponentStates.MeleeWeaponStates {
    public class MeleeWeaponStaticState : MeleeWeaponBaseState {
        public override void EnterState(MeleeWeaponStateManager meleeWeapon) {
            // meleeWeapon.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        public override void UpdateState(MeleeWeaponStateManager meleeWeapon) {
        }

        public override void OnTriggerEnter2D(MeleeWeaponStateManager meleeWeapon, Collider2D other) {
        }
    }
}