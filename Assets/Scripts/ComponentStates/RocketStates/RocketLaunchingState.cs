using Mirror;
using UnityEngine;

namespace ComponentStates.RocketStates {
    public class RocketLaunchingState : RocketBaseState {
        public override void EnterState(RocketStateManager rocket) {
            //rocket.gameObject.transform.parent = null;
            // Debug.LogWarning($"{rocket.name} Enter Launching State");
            Vector2 direction = (rocket.Head.position - rocket.Tail.position).normalized;
            var velocity = direction.normalized * RangedWeaponChildStateManager.MOVING_SPEED;
            // Debug.LogWarning($"Apply velocity to rocket: {velocity}");
            rocket.gameObject.GetComponent<Rigidbody2D>().velocity = velocity;
            
        }

        public override void UpdateState(RocketStateManager rocket) {

        }
        
        public override void OnServerTriggerEnter2D(RocketStateManager rocket, Collider2D other) {
            if(!other.CompareTag("Chassis")) return;
            var otherConnection = other.GetComponent<NetworkIdentity>().connectionToClient;
            if (otherConnection == rocket.netIdentity.connectionToClient) return;
            rocket.RpcHideChild();
            //rocket.SwitchState(rocket.explodingState);
            // Bug: On client, rocket disappear before hitting the opponent object
            // Root cause: latency of sync between server and client 
            // Fix: Hide rocket on server when hitting object, and wait
            // until rocket on client hit object and send a command to switch to exploding state
            // Debug.LogWarning($"Server Rocket hit Opponent's robot");
        }

        public override void OnClientTriggerEnter2D(RocketStateManager rocket, Collider2D other) {
            if(!other.CompareTag("Chassis")) return;
            var otherHasAuthority = other.GetComponent<NetworkIdentity>().hasAuthority;
            var thisHasAuthority = rocket.hasAuthority;
            Debug.LogWarning($"This authority: {thisHasAuthority} - Other authority: {otherHasAuthority}");
            if (otherHasAuthority == thisHasAuthority) return;
            // Debug.LogWarning($"Client Rocket hit Opponent's robot");
            rocket.CmdSwitchToExplodingState();
        }
    }
}
