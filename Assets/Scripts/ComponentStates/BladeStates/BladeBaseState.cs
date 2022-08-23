using UnityEngine;

namespace ComponentStates.BladeStates {
    public abstract class BladeBaseState {
        public abstract void EnterState(BladeStateManager blade);
        public abstract void UpdateState(BladeStateManager blade);
        public abstract void OnTriggerEnter2D(BladeStateManager blade, Collider2D other);
    }
}