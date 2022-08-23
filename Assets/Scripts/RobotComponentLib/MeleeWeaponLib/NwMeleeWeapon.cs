using ManagerLib;
using Mirror;
using UnityEngine;

namespace RobotComponentLib.MeleeWeaponLib {
    public class NwMeleeWeapon : NwWeapon {
        
        [ServerCallback]
        public void OnTriggerStay2D(Collider2D other) {
            var otherConnectionId = other.gameObject.GetComponent<NetworkIdentity>();
            var otherConnection = otherConnectionId.connectionToClient;
            var thisConnection = netIdentity.connectionToClient;
            if (thisConnection == otherConnection) return;
            var otherPlayerManager = otherConnection.identity.GetComponent<PlayerManager>();
            var otherPlayerId = otherPlayerManager.netId + 0.68f;
            Attack(otherPlayerId);
            // else {
            //     Debug.LogWarning($"Object {netId} is collider with object {other.name}");
            // }
        }
    }
}