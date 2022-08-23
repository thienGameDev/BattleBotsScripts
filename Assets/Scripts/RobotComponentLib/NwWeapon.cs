using System;
using ManagerLib;
using Mirror;
using UnityEngine;

namespace RobotComponentLib {
    public abstract class NwWeapon : NwRobotComponent {
        
        [Server]
        protected void Attack(float eventMsg) {
            var trueDamage = Damage;
            // Check if there is any additional msg. If yes, trigger melee attack method.
            var modCheck = eventMsg % 1; 
            if (modCheck != 0) {
                trueDamage = Damage / 25;
            }
            var playerId = Math.Truncate(eventMsg); // Remove additional msg.
            Debug.Log($"Damage: {Damage} - TrueDamage: {trueDamage}");
            EventManager.TriggerEvent($"TakeDamage_{playerId}", trueDamage);
        }
    }
}