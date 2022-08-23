using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace UI.Controllers {
    public class PlayerData : PersistentSingleton<PlayerData> {
        
        public Player localPlayer = new Player();
        
        // Debug
        [SerializeField] private string playerName;
        [SerializeField] private string chassisName;
        [SerializeField] private float chassisHp;
        [SerializeField] private GameObject chassisPrefab;
        [SerializeField] private List<Vector3> jointPositionsList;

        private bool _displayed;
        public struct Player {
            public string name;
            //...
            public Chassis chassis;
        }
        
        public struct Chassis {
            public string name;
            public GameObject prefab;
            public List<Vector3> jointPositions;
            public float health;
        }

        private void Update() {
            if(_displayed || localPlayer.name == null) return;
            playerName = localPlayer.name;
            chassisName = localPlayer.chassis.name;
            chassisPrefab = localPlayer.chassis.prefab;
            chassisHp = localPlayer.chassis.health;
            jointPositionsList = localPlayer.chassis.jointPositions;
            _displayed = true;
        }
    }
}