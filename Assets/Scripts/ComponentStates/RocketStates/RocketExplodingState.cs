using UnityEngine;

namespace ComponentStates.RocketStates {
    public class RocketExplodingState : RocketBaseState {
        public override void EnterState(RocketStateManager rocket) {
            // Debug.LogWarning($"{rocket.name} is exploded");
            rocket.DestroySelf();
        }

        public override void UpdateState(RocketStateManager rocket) {
  
        }

        public override void OnServerTriggerEnter2D(RocketStateManager rocket, Collider2D other) {
        }

        public override void OnClientTriggerEnter2D(RocketStateManager rocket, Collider2D other) {
        }
        
    }
}
