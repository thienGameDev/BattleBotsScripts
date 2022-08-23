using Mirror;
using UnityEngine;

namespace Utilities {
    public static class NetworkSupporter {
        public static GameObject GetSpawnedObject(uint id) {
            if (NetworkServer.spawned.TryGetValue(id, out var networkIdentityServer)) {
                return networkIdentityServer.gameObject;
            }

            if (NetworkClient.spawned.TryGetValue(id, out var networkIdentityClient)) {
                return networkIdentityClient.gameObject;
            }
            
            Debug.LogWarning($"Object {id} not found");
            return null;
        }
        
    }
}