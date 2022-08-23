using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardStates;
using Mirror;
using RobotComponentLib;
using UI;
using UnityEngine;
using Utilities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Card = UI.NwCard.Card;

namespace ManagerLib {
    public class CardDeckManager : NetWorkSingleton<CardDeckManager> {
        [SerializeField] private GameObject cardPrefab;
        private List<Card> CardPile { get; set; }
        private Object[] _weapons;
        private Object[] _gadgets;
        private Object[] _wheels;
        
        private GameObject _cardPileUI;
        private Transform _cardPileUITransform;
        
        private string _robotComponentDir;
        private string _weaponDirectory;
        private string _gadgetDirectory;
        private string _wheelDirectory;
        
        private const byte CARD_COUNT = 6; // Multiple of 3;

        public List<GameObject> UnAuthorityCards { get; private set; }

        private Dictionary<uint, NetworkIdentity> _drawnCardDictionary = new();
        private char _separator;

        protected override void Awake() {
            base.Awake();
            SetupPaths();
            CardPile = new List<Card>();
            _weapons = Resources.LoadAll(_weaponDirectory);
            _gadgets = Resources.LoadAll(_gadgetDirectory);
            _wheels = Resources.LoadAll(_wheelDirectory);
            UnAuthorityCards = new List<GameObject>();
            _cardPileUI = GameObject.FindGameObjectWithTag("CardPileUI");
            _cardPileUITransform = _cardPileUI.transform;
        }

        private void SetupPaths() {
            _separator = Path.AltDirectorySeparatorChar;
            _robotComponentDir = $"Prefabs{_separator}RobotComponent";
            _weaponDirectory = $"{_robotComponentDir}{_separator}Weapon";
            _gadgetDirectory = $"{_robotComponentDir}{_separator}Gadget";
            _wheelDirectory = $"{_robotComponentDir}{_separator}Wheel";
        }
        public override void OnStartServer() {
            base.OnStartServer();
            SpawnCards();
        }
        
        [Server]
        private void SpawnCards() {
            var numPlayer = NetworkManager.singleton.numPlayers;
            // Debug.Log($"Number of Player: {numPlayer}");
            if (numPlayer != 2) return;
            const int maxCardCount = CARD_COUNT * TurnManager.MAX_BATTLE_ROUND;
            GenerateCardPile(maxCardCount);
        }
        
        [Server]
        public void DisplayCardPileOfRound(byte roundCount) {
            var startId = (roundCount - 1)*CARD_COUNT;
            var endId = startId + CARD_COUNT;
            for (var i = startId; i < endId; i++) {
                if (startId > 0) {
                    var oldCard = CardPile[i - CARD_COUNT].cardGameObject;
                    if (UnAuthorityCards.Contains(oldCard)) {
                        NetworkServer.UnSpawn(oldCard);
                        oldCard.SetActive(false);
                        oldCard.transform.SetParent(transform);
                        UnAuthorityCards.Remove(oldCard);
                    }
                }
                GameObject cardObject = CardPile[i].cardGameObject;
                cardObject.SetActive(true);
                cardObject.transform.SetParent(_cardPileUITransform, false);
                NetworkServer.Spawn(cardObject);
                UnAuthorityCards.Add(cardObject);
            }
        }

        [Server]
        public void SetCardAuthority(NetworkIdentity playerId) {
            var connection = playerId.connectionToClient;
            // Assign authority of new cards
            foreach (var card in UnAuthorityCards) {
                NetworkIdentity cardIdentity = card.GetComponent<NetworkIdentity>();
                cardIdentity.AssignClientAuthority(connection);
                //Debug.LogWarning($"Card {card.name} is assigned authority to {connection.identity.netId}");
            }
            // Re-assign authority of drawn cards
            foreach (var drawnCard in _drawnCardDictionary) {
                var cardObject = NetworkSupporter.GetSpawnedObject(drawnCard.Key);
                if(!cardObject) continue;
                if(drawnCard.Value != playerId) continue;
                NetworkIdentity cardIdentity = cardObject.GetComponent<NetworkIdentity>();
                cardIdentity.AssignClientAuthority(connection);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdAddDrawnCard(uint cardNetId, NetworkIdentity playerId) {
            _drawnCardDictionary.Add(cardNetId, playerId);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdRemoveDrawnCard(uint cardNetId) {
            _drawnCardDictionary.Remove(cardNetId);
        }
        
        [Server]
        public void RemoveCardAuthority() {
            // Remove authority of remaining unDrawn cards
            foreach (var card in UnAuthorityCards.ToList()) {
                NetworkIdentity cardIdentity = card.GetComponent<NetworkIdentity>();
                cardIdentity.RemoveClientAuthority();
                var cardStateManager = card.GetComponent<CardStateManager>();
                if (cardStateManager.isDrawn) {
                    //Debug.Log($"Card {card.name} is removed");
                    UnAuthorityCards.Remove(card);
                }
            }
            // Remove authority of drawn cards
            foreach (var drawnCard in _drawnCardDictionary) {
                var cardObject = NetworkSupporter.GetSpawnedObject(drawnCard.Key);
                if(!cardObject) continue;
                NetworkIdentity cardIdentity = cardObject.GetComponent<NetworkIdentity>();
                cardIdentity.RemoveClientAuthority();
            }
        }
        
        [Server]
        private void GenerateCardPile(int totalCardCount) {
            int cardCount = totalCardCount / 3;
            GenerateCardPile(cardCount, _wheels, false);
            GenerateCardPile(cardCount, _weapons, true);
            //GenerateCardPile(cardCount, _weapons, true);
            GenerateCardPile(cardCount, _gadgets, false);
            ShuffleCardPile();
        }

        [Server]
        private void GenerateCardPile(int count, Object[] components, bool isAppliedDmgOnly) {
            for (int i = 0; i < count; i++) {
                var maxId = components.Length;
                var id = Random.Range(0, maxId);
                var cardTemplate = Instantiate(cardPrefab, transform);
                cardTemplate.SetActive(false);
                if (components[id] is GameObject robotComponent) {
                    cardTemplate.name = $"{cardPrefab.name}_{robotComponent.name}_{i}";
                    var nwRobotComponent = robotComponent.GetComponent<NwRobotComponent>();
                    var nwCardController = cardTemplate.GetComponent<NwCard>();
                    var componentAssetPath = GetComponentAssetPath(robotComponent);
                    Card card = nwCardController.SetupCard(componentAssetPath, isAppliedDmgOnly, nwRobotComponent.isSelfDestroyable);
                    CardPile.Add(card);
                }
                else {
                    Debug.LogError("NO ROBOT COMPONENT FOUND");
                }
            }
        }

        [Server]
        private string GetComponentAssetPath(GameObject robotComponent) {
            var componentName = robotComponent.name;
            var directories = componentName.Split(".").SkipLast(1).ToArray();
            var path = directories.Aggregate(_robotComponentDir, (current, dir) => current + $"/{dir}");
            path += $"{_separator}{componentName}";
            //Debug.Log(path);
            return path;
        }
        
        [Server]
        private void ShuffleCardPile() {
            var rnd = new System.Random();
            CardPile = CardPile.OrderBy(_ => rnd.Next()).ToList();
        }
        
    }
}

