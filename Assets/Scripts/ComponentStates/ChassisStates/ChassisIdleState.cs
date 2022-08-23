using UnityEngine;

namespace ComponentStates.ChassisStates {
    public class ChassisIdleState : ChassisBaseState {
        public override void EnterState(ChassisStateManager chassis) {
            chassis.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            // UseGrapplingRope(chassis);
        }

        private void UseGrapplingRope(ChassisStateManager chassis) {
            Debug.Log($"Grapple Point: {chassis.grapplePoint}");
            chassis.grapplingLauncher.StartGrappling(chassis.grapplePoint);
            chassis.nwChassis.Grappling(true);
        }

        public override void UpdateState(ChassisStateManager chassis) {
            
        }

        public override void OnCollisionEnter2D(ChassisStateManager chassis, Collision2D other) {
            
        }
    }
}