using UnityEngine;

namespace ComponentStates.RocketStates {
    public class RocketIdleState : RocketBaseState {
        public override void EnterState(RocketStateManager rocket) {
            Debug.Log($"{rocket.name} Enter Idle State");
            rocket.transform.position = rocket.transform.parent.position;
            rocket.transform.rotation = rocket.transform.parent.rotation;
        }
 
        public override void UpdateState(RocketStateManager rocket) {
            rocket.transform.position = rocket.transform.parent.position;
            rocket.transform.rotation = rocket.transform.parent.rotation;
        }

        public override void OnServerTriggerEnter2D(RocketStateManager rocket, Collider2D other) {
        }

        public override void OnClientTriggerEnter2D(RocketStateManager rocket, Collider2D other) {
        }
    }
}