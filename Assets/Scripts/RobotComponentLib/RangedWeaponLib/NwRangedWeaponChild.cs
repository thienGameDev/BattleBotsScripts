using ManagerLib;
using Mirror;
using UnityEngine;

namespace RobotComponentLib.RangedWeaponLib {
    public class NwRangedWeaponChild : NwWeapon {

        [ServerCallback]
        public void OnTriggerEnter2D(Collider2D other) {
            if(!other.CompareTag("Chassis")) return;
            var otherConnectionId = other.gameObject.GetComponent<NetworkIdentity>();
            var otherConnection = otherConnectionId.connectionToClient;
            var thisConnection = netIdentity.connectionToClient;
            // Debug.LogWarning($"thisConnection {thisConnection}, otherConnection {otherConnection}");
            if (thisConnection == otherConnection) return;
            var otherPlayerManager = otherConnection.identity.GetComponent<PlayerManager>();
            var otherPlayerId = otherPlayerManager.netId;
            Attack(otherPlayerId);
            // else {
            //     Debug.LogWarning($"Object {netId} is collider with object {other.name}");
            // }
        }
        
        protected override void OnSpawnClient() {
            base.OnSpawnClient();
            // transform.rotation = transform.parent.rotation;
            // if (isClientOnly && isFlipped) {
            //     FlipObject();
            // }
        }
    }
}