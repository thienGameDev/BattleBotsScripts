using System.Collections.Generic;
using Mirror;
using UI.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace ManagerLib {
    public class NetworkManagerBattleBots : NetworkManager {
        [SerializeField] private GameObject turnManagerPrefab;
        [SerializeField] private GameObject cardDeckManagerPrefab;
        [SerializeField] private Button hostBtn;
        [SerializeField] private Button joinBtn;

        private static Player _localPlayer;
        public override void OnStartServer() {
            base.OnStartServer();
            NetworkServer.RegisterHandler<Player>(OnCreatePlayer);        
        }

        [Server]
        public static Player GetPlayer() {
            return _localPlayer;
        }
        
        public override void OnStartClient() {
            base.OnStartClient();
            LoadResources();
        }
        private void OnEnable() {
            hostBtn.onClick.AddListener(CreateHost);
            joinBtn.onClick.AddListener(JoinGame);
        }
        
        private void OnDisable() {
            hostBtn.onClick.RemoveListener(CreateHost);
            joinBtn.onClick.RemoveListener(JoinGame);
        }
        
        private void MatchSetup() {
            GameObject cardDeckManager = Instantiate(cardDeckManagerPrefab);
            NetworkServer.Spawn(cardDeckManager);
            GameObject turnManager = Instantiate(turnManagerPrefab);
            NetworkServer.Spawn(turnManager);
        }
        
        [Client]
        private void LoadResources() {
            var prefabs = Resources.LoadAll("Prefabs");
            foreach (var prefab  in prefabs) {
                var gameObj = prefab as GameObject;
                if(gameObj == playerPrefab) continue;
                if(!gameObj!.TryGetComponent<NetworkIdentity>(out var component)) continue;
                NetworkClient.RegisterPrefab(gameObj);
            }
        }
        
        private void CreateHost() {
            StartHost();
        }
        
        private void JoinGame() {
            StartClient();
        }

        private void OnCreatePlayer(NetworkConnectionToClient conn, Player player) {
            _localPlayer = player;
            var playerManager = Instantiate(playerPrefab);
            playerManager.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, playerManager);
            if (numPlayers == 2) {
                MatchSetup();
            }
        }
        public override void OnClientConnect() {
            base.OnClientConnect();
            var playerData = PlayerData.Instance.localPlayer;
            var chassisData = playerData.chassis;
            var newPlayer = new Player() {
                name = playerData.name,
                chassis = new Chassis() {
                    name = chassisData.name,
                    health = chassisData.health,
                    prefabPath = PathManager.GetComponentAssetPath(chassisData.prefab),
                    jointPositions = chassisData.jointPositions
                }
            };
            NetworkClient.Send(newPlayer);
        }
        
        public struct Player : NetworkMessage
        {
            public string name;
            public Chassis chassis;
        }

        public struct Chassis {
            public string name;
            public float health;
            public string prefabPath;
            public List<Vector3> jointPositions;
        }
    }
}