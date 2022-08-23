using System.Globalization;
using UI.HUDLib;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace ManagerLib {
    public class HUDManager: Singleton<HUDManager> {
        
        [Header("Player Info Display")] 
        [SerializeField] private GameObject playerHUD;
        [SerializeField] private Text playerName;
        [SerializeField] private Text playerMaxHealthDisplay;
        [SerializeField] private Text playerTotalDamageDisplay;
        [SerializeField] private Image playerWhiteHealthBar;
        [SerializeField] private HealthBarAnim playerRedHealthBar;
        
        [Header("Opponent Info Display")] 
        [SerializeField] private GameObject opponentHUD;
        [SerializeField] private Text opponentName;
        [SerializeField] private Text opponentMaxHealthDisplay;
        [SerializeField] private Text opponentTotalDamageDisplay;
        [SerializeField] private Image opponentWhiteHealthBar;
        [SerializeField] private HealthBarAnim opponentRedHealthBar;
        
        private PlayerManager _playerManager;
        private PlayerManager _opponentManager;
        private GameObject _playerChassis;

        private void Start() {
            EventManager.StartListening("UpdatePlayerHUD", InitializePlayerHUD);
            EventManager.StartListening("UpdateOpponentHUD", InitializeOpponentHUD);
        }
        
        private void InitializePlayerHUD(float netId) {
            _playerManager = NetworkSupporter.GetSpawnedObject((uint)netId).GetComponent<PlayerManager>();
            var player = _playerManager.LocalPlayer;
            playerHUD.SetActive(true);
            playerName.text = player.name;
            playerMaxHealthDisplay.text = player.maxHealth.ToString(CultureInfo.CurrentCulture);
            playerTotalDamageDisplay.text = player.totalDamage.ToString(CultureInfo.CurrentCulture);
            RegisterPlayerEvents((int)netId);
        }
        
        private void InitializeOpponentHUD(float netId) {
            _opponentManager = NetworkSupporter.GetSpawnedObject((uint)netId).GetComponent<PlayerManager>();
            var opponent = _opponentManager.LocalPlayer;
            opponentHUD.SetActive(true);
            opponentName.text = opponent.name;
            opponentMaxHealthDisplay.text = opponent.maxHealth.ToString(CultureInfo.CurrentCulture);
            opponentTotalDamageDisplay.text = opponent.totalDamage.ToString(CultureInfo.CurrentCulture);
            RegisterOpponentEvents((int)netId);
        }
        
        private void RegisterPlayerEvents(int netId) {
            CreateEventName(netId, out var eventUpdateHealthBar, out var eventUpdateHUDTotalDamage);
            EventManager.StartListening(eventUpdateHealthBar, UpdatePlayerHealthBar);
            EventManager.StartListening(eventUpdateHUDTotalDamage, UpdatePlayerTotalDamage);
        }

        private void CreateEventName(int netId, out string eventUpdateHealthBar, out string eventUpdateTotalDamage) {
            eventUpdateHealthBar = $"HUDHealthBar_{netId}";
            eventUpdateTotalDamage = $"HUDTotalDamage_{netId}";
        }
        
        private void RegisterOpponentEvents(int netId) {
            CreateEventName(netId, out var eventUpdateHealthBar, out var eventUpdateTotalDamage);
            EventManager.StartListening(eventUpdateHealthBar, UpdateOpponentHealthBar);
            EventManager.StartListening(eventUpdateTotalDamage, UpdateOpponentTotalDamage);
        }
        
        private void UpdatePlayerHealthBar(float currentHealth) {
            var maxHealth = _playerManager.LocalPlayer.maxHealth;
            var targetFillAmount = currentHealth / maxHealth;
            playerWhiteHealthBar.fillAmount = targetFillAmount;
            playerRedHealthBar.TargetFillAmount = targetFillAmount;
            playerMaxHealthDisplay.text = maxHealth.ToString(CultureInfo.CurrentCulture);
        }
        
        private void UpdateOpponentHealthBar(float currentHealth) {
            var maxHealth = _opponentManager.LocalPlayer.maxHealth;
            var targetFillAmount = currentHealth / maxHealth;
            opponentWhiteHealthBar.fillAmount = targetFillAmount;
            opponentRedHealthBar.TargetFillAmount = targetFillAmount;
            opponentMaxHealthDisplay.text = maxHealth.ToString(CultureInfo.CurrentCulture);
        }
        
        private void UpdatePlayerTotalDamage(float totalDamage) {
            playerTotalDamageDisplay.text = totalDamage.ToString(CultureInfo.CurrentCulture);
        }
        private void UpdateOpponentTotalDamage(float totalDamage) {
            opponentTotalDamageDisplay.text = totalDamage.ToString(CultureInfo.CurrentCulture);
        }
    }
}