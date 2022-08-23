using UnityEngine;

namespace ComponentStates.BladeStates {
    public class BladeIdleState : BladeBaseState {
        public override void EnterState(BladeStateManager blade) {
            blade.gameObject.GetComponent<WheelJoint2D>().useMotor = false;
        }

        public override void UpdateState(BladeStateManager blade) {
            
        }

        public override void OnTriggerEnter2D(BladeStateManager blade, Collider2D other) {
            
        }
    }
}