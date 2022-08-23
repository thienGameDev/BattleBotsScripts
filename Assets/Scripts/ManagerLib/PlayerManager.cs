using EnumTypes;
using Interfaces;
using Mirror;
using RobotComponentLib;
using RobotComponentLib.ChassisLib;
using UnityEngine;

namespace ManagerLib {
    public class PlayerManager : NetworkBehaviour {
        
        [SyncVar(hook = nameof(OnMaxHealthChange))] private float _maxHealth;
        [SyncVar(hook = nameof(OnCurrentHealthChange))] private float _currentHealth;
        [SyncVar(hook = nameof(OnTotalDamageChange))] private float _totalDamage;
        [SyncVar] private uint _chassisNetId;
        [SyncVar] private GameObject _playerChassis;
        [SyncVar] private string _playerName;

        public GameObject chassisPrefab;
        public GameObject selectedChassis;
        private Camera _cam;
        private bool _camFlipped;
        private string _eventTakeDamage;
        private string _eventUpdateHealthBar;
        private string _eventUpdateTotalDamage;
        public Player LocalPlayer => GetLocalPlayer();

        private Vector3 _playerChassisPosition;
        private Vector3 _opponentChassisPosition;
        private Vector2 _playerGrapplePoint;
        private Vector2 _opponentGrapplePoint;
        private NetworkManagerBattleBots.Player _player;
        private Vector3 _velocity = Vector3.zero;
        private float _vel = 0f;
        private float _smoothTime = 0.5f;
        public bool isResetPlayerPosition;
        public bool isResetOpponentPosition;
        private void Awake() {
            _playerChassisPosition = GameObject.FindGameObjectWithTag("ChassisPositionLeft").transform.localPosition;
            _opponentChassisPosition = GameObject.FindGameObjectWithTag("ChassisPositionRight").transform.localPosition;
            // _playerGrapplePoint = GameObject.FindGameObjectWithTag("GrapplePointLeft").transform.localPosition;
            // _opponentGrapplePoint = GameObject.FindGameObjectWithTag("GrapplePointRight").transform.localPosition;
        }
        
        public struct Player {
            public string name;
            public float maxHealth;
            public float totalDamage;
            public uint chassisNetId;
        }
        
        #region Server

        public override void OnStartServer() {
            base.OnStartServer();
            GetPlayerData();
            InstantiateChassisOnBattleView();
            SpawnChassis();
            InitializePlayer();
            RegisterServerEvent();
        }

        [Server]
        private void GetPlayerData() {
            _player = NetworkManagerBattleBots.GetPlayer();
            var chassisPrefabPath = _player.chassis.prefabPath;
            chassisPrefab = Resources.Load(chassisPrefabPath) as GameObject;
        }
        
        [Server]
        public void EnterDynamicState() {
            if(!_playerChassis) return;
            _playerChassis.GetComponent<IFreezable>().Unfreeze();
            Debug.Log($"{netId} is enter dynamic state");
        }
        
        [Server]
        public void EnterStaticState() {
            if(!_playerChassis) return;
            _playerChassis.GetComponent<IFreezable>().Freeze();
            Debug.Log($"{netId} is enter static state");
        }

        [Server]
        private void RegisterServerEvent() {
            _eventTakeDamage = $"TakeDamage_{netId}";
            EventManager.StartListening(_eventTakeDamage, ReduceHealth);
        }

        [Server]
        public void StopTakingDamage() {
            EventManager.StopListening(_eventTakeDamage, ReduceHealth);
        }
        
        [Server]
        private void ResetPositionToTheLeft() {
            if(!_playerChassis) return;
            Debug.LogWarning("Reset player chassis position to the left");
            var chassisTransform = _playerChassis.transform;
            var currentPos = chassisTransform.position;
            var currentRotation = chassisTransform.rotation;
            chassisTransform.position = Vector3.SmoothDamp(currentPos,_playerChassisPosition,ref _velocity,_smoothTime);
            currentRotation.z = Mathf.SmoothDamp(currentRotation.z, 0f, ref _vel, _smoothTime);
            chassisTransform.rotation = currentRotation;
            if (chassisTransform.rotation.z == 0) isResetPlayerPosition = false;
        }
        
        
        [Server]
        private void ResetPositionToTheRight() {
            if(!_playerChassis) return;
            Debug.LogWarning("Reset player chassis position to the right");
            var chassisTransform = _playerChassis.transform;
            var currentPos = chassisTransform.position;
            var currentRotation = chassisTransform.rotation;
            chassisTransform.position = Vector3.SmoothDamp(currentPos,_opponentChassisPosition, ref _velocity, _smoothTime);
            currentRotation.z = Mathf.SmoothDamp(currentRotation.z, 0f, ref _vel, _smoothTime);
            chassisTransform.rotation = currentRotation;
            if (chassisTransform.rotation.z == 0f) isResetOpponentPosition = false;
        }
        
        private void Update() {
            if(!isServer) return;
            if(isResetPlayerPosition)
                ResetPositionToTheLeft();
            if(isResetOpponentPosition)
                ResetPositionToTheRight();
        }

        [Server]
        private void SpawnChassis() {
            NetworkServer.Spawn(selectedChassis, connectionToClient);
            _playerChassis = selectedChassis;
        }

        [Server]
        private void InstantiateChassisOnBattleView() {
            var battleView = GameObject.FindGameObjectWithTag("BattleView");
            selectedChassis = Instantiate(chassisPrefab, battleView.transform, false);
            var nwChassis = selectedChassis.GetComponent<NwChassis>();
            nwChassis.InitializeChassis(_player.chassis.jointPositions);
            SetPosition();
        }

        [Server]
        private void InitializePlayer() {
            _playerName = _player.name;
            _maxHealth = _player.chassis.health;
            _currentHealth = _maxHealth;
            _totalDamage = 0f;
            _chassisNetId = _playerChassis.GetComponent<NetworkIdentity>().netId;
        }
        
        [Server]
        private void ReduceHealth(float damage) {
            if (_currentHealth > damage) {
                _currentHealth -= damage;
                Debug.Log($"Player {netId} Damaged: {_currentHealth}/{_maxHealth}");
            }
            else {
                _currentHealth = 0;
                Destroy(_playerChassis);
                EventManager.StopListening(_eventTakeDamage, ReduceHealth);
                EventManager.TriggerEvent(GameEvent.PlayerDead.ToString(), netId);
            }
        }
        
        [Server]
        private void SetPosition() {
            var numPlayers = NetworkManager.singleton.numPlayers;
            selectedChassis.transform.localPosition = _playerChassisPosition;
            selectedChassis.GetComponent<NwChassis>().GrapplePoint = _playerGrapplePoint;
            if (numPlayers <2) return;
            selectedChassis.GetComponent<NwRobotComponent>().FlipObject(); 
            selectedChassis.GetComponent<NwRobotComponent>().MirrorObject();
            selectedChassis.GetComponent<NwChassis>().GrapplePoint = _opponentGrapplePoint;
            selectedChassis.GetComponent<NwRobotComponent>().isFlipped = true;
        }

        [Server]
        private void UpdatePlayerTotalDamage(float dmg) {
            _totalDamage += dmg;
        }
        
        [Server]
        private void UpdatePlayerMaxHealth(float hp) {
            _maxHealth += hp;
            _currentHealth += hp;
        }
        
        [Command(requiresAuthority = true)]
        public void CmdAttachComponentOnChassis(byte jointId, string componentPath, float damage, float health) {
            var componentPrefab = Resources.Load(componentPath) as GameObject;
            var nwChassis = _playerChassis.GetComponent<NwChassis>();
            var chassisTransform = _playerChassis.transform;
            var component = Instantiate(componentPrefab, chassisTransform, false);
            component.GetComponent<NwRobotComponent>().Damage = damage;
            nwChassis.AttachRobotComponentAt(jointId, component);
            UpdatePlayerTotalDamage(damage);
            UpdatePlayerMaxHealth(health);
        }

        [Command(requiresAuthority = true)]
        public void CmdDetachComponentOnChassis(byte jointId, float damage, float health) {
            var nwChassis = _playerChassis.GetComponent<NwChassis>();
            nwChassis.DetachRobotComponentAt(jointId);
            UpdatePlayerTotalDamage(-damage);
            UpdatePlayerMaxHealth(-health);
        }
        
        #endregion

        private Player GetLocalPlayer() {
            var localPlayer = new Player() {
                name = _playerName,
                chassisNetId = _chassisNetId,
                maxHealth = _maxHealth,
                totalDamage = _totalDamage
            };
            return localPlayer;
        }
        
        #region Client
        
        // public void OnPlayerUpdate(Player _old, Player _new) {
        //     if(hasAuthority)
        //         EventManager.TriggerEvent("UpdatePlayerHUD", netId);
        // }
        
        public override void OnStartClient() {
            RegisterClientEvent();
            EventManager.TriggerEvent(hasAuthority ? "UpdatePlayerHUD" : "UpdateOpponentHUD", netId);
            if(_camFlipped) return;
            //FlipCamera();
           if(isClientOnly) FlipBattleView();
            _camFlipped = true;
        }

        [Client]
        private void RegisterClientEvent() {
            _eventUpdateHealthBar = $"HUDHealthBar_{netId}";
            _eventUpdateTotalDamage = $"HUDTotalDamage_{netId}";
            
        }
        
        public override void OnStopClient() {
            base.OnStopClient();
            _camFlipped = false;
            Debug.Log($"Player {netId}: OnStopClient");
        }

        [Client]
        private void FlipBattleView() {
            var battleView = GameObject.FindGameObjectWithTag("BattleView");
            var scale = battleView.transform.localScale;
            scale.x *= -1;
            battleView.transform.localScale = scale;
        }

        [Client]
        private void OnMaxHealthChange(float oldValue, float newValue) {
            if(oldValue == 0) return;
            var deltaHp = newValue - oldValue;
            var newCurrentHealth = _currentHealth + deltaHp;
            EventManager.TriggerEvent(_eventUpdateHealthBar, newCurrentHealth);
        }
        
        [Client]
        private void OnCurrentHealthChange(float oldValue, float newValue) {
            if(oldValue == 0) return;
            Debug.Log($"Old current hp: {oldValue}");
            Debug.Log($"New current hp: {newValue}");
            EventManager.TriggerEvent(_eventUpdateHealthBar, newValue);
        }
        
        [Client]
        private void OnTotalDamageChange(float oldValue, float newValue) {
            EventManager.TriggerEvent(_eventUpdateTotalDamage, newValue);
        }
        
        #endregion
    }
}
