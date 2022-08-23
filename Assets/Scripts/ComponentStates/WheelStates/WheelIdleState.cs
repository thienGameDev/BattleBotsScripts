using UnityEngine;

namespace ComponentStates.WheelStates {
    public class WheelIdleState : WheelBaseState {
        public override void EnterState(WheelStateManager wheel) {
            var rigidBody = wheel.gameObject.GetComponent<Rigidbody2D>();
            rigidBody.bodyType = RigidbodyType2D.Static;
        }
        
        public override void UpdateState(WheelStateManager wheel) {
            
        }

        public override void OnCollisionEnter2D(WheelStateManager wheel, Collision2D other) {
            
        }

        public override void OnCollisionExit2D(WheelStateManager wheel, Collision2D other) {

        }
    }
}