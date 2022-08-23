using UnityEngine;

namespace ComponentStates.ChassisStates {
    public abstract class ChassisBaseState {
        public abstract void EnterState(ChassisStateManager chassis);
        public abstract void UpdateState(ChassisStateManager chassis);
        public abstract void OnCollisionEnter2D(ChassisStateManager chassis, Collision2D other);
    }
}