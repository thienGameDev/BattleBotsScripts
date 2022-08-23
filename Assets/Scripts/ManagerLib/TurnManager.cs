using System.Collections;
using _Camera;
using EnumTypes;
using Mirror;
using ObstacleLib;
using RobotComponentLib.ChassisLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace ManagerLib {
    public class TurnManager : NetWorkSingleton<TurnManager> {
        [SerializeField] private float healthWeight = .35f;
        [SerializeField] private float damageWeight = .35f;
        [SerializeField] private float energyWeight = .2f;
        [SerializeField] private float jointSlotCountWeight = .1f;

        // Sync Variables
        [SyncVar] private NetworkIdentity _currentPlayerId;
        [SyncVar] private NetworkIdentity _nextPlayerId;
        [SyncVar] private uint _loserId;
        [SyncVar(hook = nameof(OnCurrentPlayerEnergyCountChange))] private byte _currentPlayerEnergyCount;
        [SyncVar] private byte _battleRoundCount;
        [SyncVar(hook = nameof(OnTurnCountChange))] private byte _turnCount;
        private readonly SyncDictionary<NetworkIdentity, byte> _maxPlayerEnergyCount = new ();
        
        // UI elements
        private TMP_Text _roundInfoText;
        private TMP_Text _countDownTimerText;
        private GameObject _preBattleUI;
        private TMP_Text _playerTurnText;
        private TMP_Text _energyCountText;
        private GameObject _energyCountUI;
        private GameObject _energyCountBackground;
        public Button endTurnBtn;

        private GameObject _preBattleObstacle;
        private GameObject _inBattleObstacle;
        private GameObject _endBattleObstacle;
        
        // Constant variables
        private const string TURN_TIMER = "TU_TI";
        private const string BATTLE_TIMER = "RO_TI";
        private const int BATTLE_DURATION = 15;
        private const int TURN_DURATION = 20;
        public const int MAX_BATTLE_ROUND = 5;
        private const byte FIRST_BATTLE_ENERGY = 3;
        private const byte NEXT_BATTLE_ENERGY = 2;
        
        // Runtime variables
        private PlayerManager _player;
        private PlayerManager _opponent;
        private NetworkIdentity _localPlayerId;
        private Coroutine _currentClientTimer;
        private Coroutine _currentServerTimer;
        private int _countDownTime;
        private CardDeckManager _cardDeckManager = CardDeckManager.Instance;
        private Camera _cam;
        private OrthographicZoom _orthographicZoom;
        
        #region Server
        private void Start() {
            _cam = Camera.main;
            _orthographicZoom = _cam!.GetComponent<OrthographicZoom>();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            RegisterEvents();
            CreateNewRound();
        }

        [Server]
        private void RegisterEvents() {
            EventManager.StartListening(GameEvent.PlayerDead.ToString(), EndBattle);
        }

        [Command(requiresAuthority = false)]
        private void CmdStartServerTimer(NetworkIdentity playerId, int duration, string timerName) {
            _currentServerTimer = StartCoroutine(ServerTimer(playerId, duration, timerName));
            
        }
        
        [Server]
        private IEnumerator ServerTimer(NetworkIdentity playerId, int duration, string timerName) {
            var now = NetworkTime.time;
            var serverStopTime = now + duration;
            while (now < serverStopTime) {
                yield return new WaitForSeconds(1f);
                now = NetworkTime.time;
            }
            // Debug.LogWarning($"Server Timer stopped at: {now}");
            switch (timerName) {
                case BATTLE_TIMER:
                    // Proceed Battle result
                    if (_battleRoundCount == MAX_BATTLE_ROUND) {
                        RpcApplyPassiveDamage();
                    }
                    else {
                        EndBattle(_loserId);
                    }
                    break;
                case TURN_TIMER:
                    // ProceedEndTurnRequest
                    EndTurn(playerId);
                    break;
                default:
                    Debug.LogError($"Timer Name is wrong!");
                    break;
            } 
        }

        [Server]
        private void EndBattle(float playerManagerNetId) {
            if (playerManagerNetId > 0) {
                StopCoroutine(_currentServerTimer);
                RpcStopClientTimer();
                _loserId = (uint)playerManagerNetId;
                //Stop Damaging the winner
                StopDamageWinner();
                RpcDisplayBattleResult();
            }
            else {
                RpcSetPreBattleObstacles(true);
                RpcSetInBattleObstacleActive(false);
                RpcActivateBattleCamera(false);
                ResetRobots();
                CreateNewRound();
            }
        }

        [Server]
        private void StopDamageWinner() {
            var winnerId = _currentPlayerId.netId == _loserId ? _nextPlayerId : _currentPlayerId;
            var winner = winnerId.connectionToClient.identity.GetComponent<PlayerManager>();
            winner.StopTakingDamage();
        }
        
        [Command(requiresAuthority = false)]
        private void CmdProceedEndTurnRequest(NetworkIdentity playerId) {
            StopCoroutine(_currentServerTimer);
            EndTurn(playerId);
        }

        [Server]
        private void EndTurn(NetworkIdentity playerId) {
            UpdateRemainingEnergyCount(playerId);
            _turnCount++; // Trigger Event OnTurnCountChange on Client Sides
            if (_turnCount % 2 != 0) return;
            // Remove authority of remaining cards
            _cardDeckManager.RemoveCardAuthority();
            // Start Battle on Server
            StartBattle();
        }

        [Server]
        private void StartBattle() {
            Debug.Log("Battle started on server");
            // Release Robot -> Unfreeze Chassis -> Apply physics
            ReleaseRobots();
            // Active Orthographic Zoom on Main Camera
            RpcActivateBattleCamera(true);
            //Start Battle Timer
            StartCoroutine(ServerTimer(_localPlayerId, BATTLE_DURATION, BATTLE_TIMER));
            // Sync Battle Timer to clients
            RpcStartBattleTimer();
            // Release InBattle obstacles
            RpcSetInBattleObstacleActive(true);
            // Remove PreBattle obstacles
            RpcSetPreBattleObstacles(false);
        }
        
        [Server]
        private void ReleaseRobots() {
            _player.EnterDynamicState();
            _opponent.EnterDynamicState();
        }
        
        [Server]
        private void ResetRobots() {
            _player.EnterStaticState();
            _opponent.EnterStaticState();
            _player.isResetPlayerPosition = true;
            _opponent.isResetOpponentPosition = true;
        }

        [Server]
        private void ManageTurn() {
            var playerList = FindObjectsOfType<PlayerManager>();
            // Debug.Log($"Object {netId} called ManageTurn. player List count: {playerList.Length}");
            foreach (var player in playerList) {
                if (player.isLocalPlayer) {
                    // Debug.Log("Player updated");
                    _player = player;
                }
                else {
                    // Debug.Log("Opponent updated");
                    _opponent = player;
                }
                DistributeEnergy(player.netIdentity);
            }
            var playerScore = TotalScore(_player);
            // Debug.Log($"Player score: {playerScore}");
            var opponentScore = TotalScore(_opponent);
            // Debug.Log($"Opponent score: {opponentScore}");
            _currentPlayerId = playerScore <= opponentScore ? _player.netIdentity : _opponent.netIdentity;
            _nextPlayerId = playerScore > opponentScore ? _player.netIdentity : _opponent.netIdentity;
            // Debug.LogWarning($"Current player turn conn Id: {_currentPlayerId}");
            // Debug.LogWarning($"Next player turn conn Id: {_nextPlayerId}");
        }
        
        [Server]
        private float TotalScore(PlayerManager playerManager) {
            var player = playerManager.LocalPlayer;
            // Criteria: Health, Damage, Energy, JointSlotCount
            var playerChassis = NetworkSupporter.GetSpawnedObject(player.chassisNetId);
            var healthScore = player.maxHealth * healthWeight;
            var damageScore = player.totalDamage * damageWeight;
            var energyScore = _maxPlayerEnergyCount[playerManager.netIdentity] * energyWeight;
            var jointSlotCountScore = playerChassis.GetComponent<NwChassis>().jointDict.Count * jointSlotCountWeight;
            return healthScore + damageScore + energyScore + jointSlotCountScore;
        }

        [Server]
        private void CreateNewRound() {
            _battleRoundCount++;
            ManageTurn();
            RpcStartNewRound();
            SpawnCard(); // Must be the last caller
        }
        
        [Server]
        private void SpawnCard() {
            _cardDeckManager.DisplayCardPileOfRound(_battleRoundCount);
        }
        
        [Server]
        private void DistributeEnergy(NetworkIdentity playerId) {
            var energy = _turnCount == 0 ? FIRST_BATTLE_ENERGY : NEXT_BATTLE_ENERGY;
            if(_maxPlayerEnergyCount.ContainsKey(playerId))
                _maxPlayerEnergyCount[playerId] += energy;
            else {
                _maxPlayerEnergyCount.Add(playerId, energy);
            }
            //Debug.Log($"Server: Player {playerId.netId} has been added {energy} Energy");
            //Fixed Bug: Energy is not update for client if isClientOnly player start first
        }
        
        [Command(requiresAuthority = false)]
        private void CmdSetCardAuthorityTo(NetworkIdentity localPlayerId) {
            _cardDeckManager.RemoveCardAuthority();
            _cardDeckManager.SetCardAuthority(localPlayerId);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdSubtractEnergy(byte energy) {
            _currentPlayerEnergyCount -= energy;
        }
        
        [Command(requiresAuthority = false)]
        private void CmdAddEnergy(byte energy) {
            _currentPlayerEnergyCount += energy;
        }
        
        [Server]
        private void UpdateRemainingEnergyCount(NetworkIdentity playerId) {
            if (_maxPlayerEnergyCount.ContainsKey(playerId)) {
                _maxPlayerEnergyCount[playerId] = _currentPlayerEnergyCount;
            }
            else {
                Debug.LogError($"Identity {playerId} not found!");
            }
        }
        
        [Command(requiresAuthority = false)]
        private void CmdUpdateCurrentPlayerEnergyCount(byte energy) {
            _currentPlayerEnergyCount = energy;
        }
        
        [Command(requiresAuthority = false)]
        private void CmdStartTurn(NetworkIdentity playerId) {
            if (_turnCount % 2 != 0) {
                // Transfer turn 
                (_currentPlayerId, _nextPlayerId) = (_nextPlayerId, _currentPlayerId);
            }
            TargetStartTurnOnClient(playerId.connectionToClient);
            // Debug.LogWarning($"Turn {_turnCount} is started");
            
        }

        #endregion

        #region Client
        public override void OnStartClient() {
            base.OnStartClient();
            _preBattleObstacle = GameObject.FindGameObjectWithTag("PreBattleObstacle");
            _inBattleObstacle = GameObject.FindGameObjectWithTag("InBattleObstacle");
            _endBattleObstacle = GameObject.FindGameObjectWithTag("EndBattleObstacle");
            _roundInfoText = GameObject.FindGameObjectWithTag("RoundInfoText").GetComponent<TMP_Text>();
            _countDownTimerText = GameObject.FindGameObjectWithTag("CountDownTimerText").GetComponent<TMP_Text>();
            _preBattleUI = GameObject.FindGameObjectWithTag("PreBattleUI");
            _energyCountUI = GameObject.FindGameObjectWithTag("EnergyCountUI");
            _energyCountBackground = _energyCountUI.transform.Find("Background").gameObject;
            _playerTurnText = GameObject.FindGameObjectWithTag("PlayerTurnText").GetComponent<TMP_Text>();
            _energyCountText = _energyCountUI.transform.Find("EnergyCountText").GetComponent<TMP_Text>();
            endTurnBtn = _energyCountUI.transform.parent.Find("EndTurnBtn").GetComponent<Button>();
            endTurnBtn.onClick.AddListener(EndTurn);
            GetLocalPlayerIdentity();
            CmdStartTurn(_localPlayerId);
        }

        [Client]
        public bool SubtractEnergy(byte energy) {
            if (energy > _currentPlayerEnergyCount) {
                // Debug.LogWarning($"Not enough energy: {energy}/{_currentPlayerEnergyCount}");
                return false;
            }
            CmdSubtractEnergy(energy);
            return true;
        }
        
        [Client]
        public void AddEnergy(byte energy) {
            CmdAddEnergy(energy);
        }
        
        [Client]
        private void GetLocalPlayerIdentity() {
            var networkIdentity = NetworkClient.connection.identity;
            _localPlayerId = networkIdentity.GetComponent<PlayerManager>().netIdentity;
        }
        
        [Client]
        private IEnumerator ClientTimer(int duration) {
            _countDownTime = duration;
            _countDownTimerText.text = _countDownTime.ToString();
            var now = NetworkTime.time;
            var clientStopTime = now + duration;
            while (now < clientStopTime) {
                yield return new WaitForSeconds(1f);
                _countDownTime--;
                _countDownTimerText.text = _countDownTime.ToString();
                now = NetworkTime.time;
            }
            _countDownTimerText.text = "";
            // Debug.Log($"Client Timer stopped at: {now}");

        }

        [Client]
        private void ShowEndTurnButton(bool shown) {
            endTurnBtn.gameObject.SetActive(shown);
        }
        
        [Client]
        private void ShowEnergyCountUI(bool shown) {
            _energyCountBackground.SetActive(shown);
            _energyCountText.gameObject.SetActive(shown);
        }

        [Client]
        private void ShowPreBattleUI(bool shown) {
            _preBattleUI.SetActive(shown);
        }
        
        [ClientRpc]
        private void RpcStartNewRound() {
            ShowPreBattleUI(true);
            CmdStartTurn(_localPlayerId);
        }

        [ClientRpc]
        private void RpcSetInBattleObstacleActive(bool active) {
            var movableObstacles = _inBattleObstacle.GetComponentsInChildren<MovableObstacle>(true);
            foreach (var obstacle in movableObstacles) {
                obstacle.enabled = active;
            }
        }
        
        [ClientRpc]
        private void RpcSetPreBattleObstacles(bool active) {
            var movableObstacles = _preBattleObstacle.GetComponentsInChildren<MovableObstacle>(true);
            foreach (var obstacle in movableObstacles) {
                if (active) obstacle.enabled = true;
                else obstacle.ReverseTranslation();
            }
        }

        [ClientRpc]
        private void RpcActivateBattleCamera(bool active) {
            if (!active) {
                _cam.transform.position = Vector3.zero;
                _cam.orthographicSize = 6.2f;
            }
            _orthographicZoom.enabled = active;
        }
        
        [TargetRpc]
        private void TargetStartTurnOnClient(NetworkConnection target) {
            if (_localPlayerId == _currentPlayerId) {
                // Allow this player to draw cards
                CmdUpdateCurrentPlayerEnergyCount(_maxPlayerEnergyCount[_localPlayerId]);
                CmdStartServerTimer(_localPlayerId, TURN_DURATION, TURN_TIMER);
                _currentClientTimer = StartCoroutine(ClientTimer(TURN_DURATION));
                CmdUpdateCurrentPlayerEnergyCount(_maxPlayerEnergyCount[_localPlayerId]);
                DisplayTurnInfoUI(true);
                ShowEnergyCountUI(true);
                ShowEndTurnButton(true);
                CmdSetCardAuthorityTo(_localPlayerId);
            } else if (_localPlayerId == _nextPlayerId) {
                // Ask this player to wait for the opponent
                DisplayTurnInfoUI(false);
                ShowEnergyCountUI(false);
                ShowEndTurnButton(false);
            }
            else {
                Debug.LogError($"Player Id not found: {_localPlayerId}");
            }
        }

        [ClientRpc]
        private void RpcStartBattleTimer() {
            _currentClientTimer = StartCoroutine(ClientTimer(BATTLE_DURATION));
            
        }
        
        [Client]
        private void DisplayTurnInfoUI(bool isPlayerTurn) {
            _roundInfoText.text = $"BATTLE {_battleRoundCount}";
            if (isPlayerTurn) {
                _playerTurnText.text = "YOUR TURN";
                var maxEnergyCount = _maxPlayerEnergyCount[_localPlayerId];
                _energyCountText.text = $"{maxEnergyCount}/{maxEnergyCount}";
            }
            else {
                _playerTurnText.text = "WAIT FOR YOUR OPPONENT";
                _energyCountText.text = "";
                _countDownTimerText.text = "";
            }
        }
        
        [Client]
        public void EndTurn() {
            StopCoroutine(_currentClientTimer);
            CmdProceedEndTurnRequest(_localPlayerId);
            // Debug.Log("EndTurn is clicked");
        }

        [ClientRpc]
        private void RpcStopClientTimer() {
            StopCoroutine(_currentClientTimer);
        }
        
        [ClientRpc]
        private void RpcApplyPassiveDamage() {
            // Trigger end battle obstacles
            var movableObstacle = _endBattleObstacle.GetComponentInChildren<MovableObstacle>();
            movableObstacle.enabled = true;
        }
        
        [ClientRpc]
        private void RpcDisplayBattleResult() {
            ShowPreBattleUI(false);
            var postBattleUI = GameObject.FindGameObjectWithTag("PostBattleUI");
            var childCount = postBattleUI.transform.childCount;
            for (var id = 0; id < childCount; id++) {
                postBattleUI.transform.GetChild(id).gameObject.SetActive(true);
            }
        }

        [Client]
        public bool IsWinner() {
            return _localPlayerId.netId != _loserId;
        }
        [Client]
        private void OnTurnCountChange(byte oldTurnCount, byte newTurnCount) {
            if (newTurnCount % 2 == 0) {
                // Start battle on Client
                Debug.Log("Battle started on client");
                ShowPreBattleUI(false);
            }
            else {
                // Transfer turn to other player
                ShowEnergyCountUI(false);
                ShowEndTurnButton(false);
                CmdStartTurn(_localPlayerId); 
                // Debug.LogWarning("CmdStart Turn is called");
            }
        }

        [Client]
        private void OnCurrentPlayerEnergyCountChange(byte oldEnergyCount, byte newEnergyCount) {
            if (_energyCountText.IsActive()) {
                _energyCountText.text = $"{newEnergyCount.ToString()}/{_maxPlayerEnergyCount[_localPlayerId]}";
            }
        }
        
        #endregion
    }
}
