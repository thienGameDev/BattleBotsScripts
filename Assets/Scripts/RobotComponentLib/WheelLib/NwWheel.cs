using ManagerLib;
using Mirror;
using UnityEngine;

namespace RobotComponentLib.WheelLib {
    public class NwWheel : NwRobotComponent {
        protected override void OnSpawnServer() {
            base.OnSpawnServer();
        }

        protected override void OnSpawnClient() {
            base.OnSpawnClient();
        }
        
        [ServerCallback]
        private void OnCollisionEnter2D(Collision2D col) {
            if (col.gameObject.CompareTag("Obstacle")) {
                var playerNetId = connectionToClient.identity.GetComponent<PlayerManager>().netId;
                EventManager.TriggerEvent($"TakeDamage_{playerNetId}", DAMAGE_BY_OBSTACLE);
                Debug.Log("Hit Obstacle");
            }
        }
    }
}