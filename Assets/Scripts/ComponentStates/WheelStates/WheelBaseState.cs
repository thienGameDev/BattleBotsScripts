using UnityEngine;

namespace ComponentStates.WheelStates {
    public abstract class WheelBaseState {
        public abstract void EnterState(WheelStateManager wheel);
        public abstract void UpdateState(WheelStateManager wheel);
        public abstract void OnCollisionEnter2D(WheelStateManager wheel, Collision2D other);
        public abstract void OnCollisionExit2D(WheelStateManager wheel, Collision2D other);
    }
}