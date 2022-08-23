using UnityEngine;

namespace ComponentStates.ChassisStates {
    public class ChassisOnDamagedState: ChassisBaseState {
        public override void EnterState(ChassisStateManager chassis) {
            
        }

        public override void UpdateState(ChassisStateManager chassis) {
            
        }

        public override void OnCollisionEnter2D(ChassisStateManager chassis, Collision2D other) {
            throw new System.NotImplementedException();
        }
    }
}