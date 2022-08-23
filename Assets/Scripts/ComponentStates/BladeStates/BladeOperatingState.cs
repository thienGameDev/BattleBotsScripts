using UnityEngine;

namespace ComponentStates.BladeStates {
    public class BladeOperatingState : BladeBaseState {
        public override void EnterState(BladeStateManager blade) {
            blade.gameObject.GetComponent<WheelJoint2D>().useMotor = true;
        }

        public override void UpdateState(BladeStateManager blade) {
            
        }

        public override void OnTriggerEnter2D(BladeStateManager blade, Collider2D other) {
            
        }
    }
}