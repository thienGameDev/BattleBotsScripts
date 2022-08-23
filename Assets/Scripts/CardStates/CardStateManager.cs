using ManagerLib;
using Mirror;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CardStates {
    public class CardStateManager : NetworkBehaviour, IDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler {
        [SyncVar] public bool isDrawn;
        private CardBaseState _currentState;
        private CardBaseState _drawableState = new CardDrawableState();
        private CardBaseState _undrawableState = new CardUndrawableState();
        private CardBaseState _selectedState = new CardSelectedState();
        private GameObject _cardPileUI;
        private GameObject _jointCardUI;
        private GameObject _playerCardUI;
        private Transform _cardPileUITransform;
        private Transform _jointCardUITransform;
        private Transform _playerCardUITransform;
        private NetworkIdentity _playerId;
        private PlayerManager _playerManager;
        public NwJointCard OccupiedJointCard  { get; set; }
        
        public bool isUsedInBattle;
        private NwCard Card { get; set; }
        
        public override void OnStartClient() {
            base.OnStartClient();
            _cardPileUI = GameObject.FindGameObjectWithTag("CardPileUI");
            _jointCardUI = GameObject.FindGameObjectWithTag("JointCardUI");
            _playerCardUI = GameObject.FindGameObjectWithTag("PlayerCardUI");
            _cardPileUITransform = _cardPileUI.transform;
            _jointCardUITransform = _jointCardUI.transform;
            _playerCardUITransform = _playerCardUI.transform;
            Card = GetComponent<NwCard>();
            _currentState = _undrawableState;
            _currentState.EnterState(this);
            _playerId = NetworkClient.connection.identity;
            _playerManager = _playerId.GetComponent<PlayerManager>();
        }

        public override void OnStartAuthority() {
            base.OnStartAuthority();
            SwitchState(isDrawn ? _selectedState : _drawableState);
            // if(_currentState == _selectedState)
                // Debug.LogWarning($"On start authority Card {name} enter selectedState");
        }

        public override void OnStopAuthority() {
            base.OnStopAuthority();
            isUsedInBattle = true;
            SwitchState(_undrawableState);
            // Debug.LogWarning("Card is remove authority");
        }
        
        [Client]
        private void SwitchState(CardBaseState newState) {
            ResetCard();
            _currentState = newState;
            _currentState.EnterState(this);
        }
        
        [Client]
        public void GlowCard(bool on) {
            transform.Find("Background").GetComponent<Outline>().enabled = on;
        }
        
        [Client]
        public void ScaleCard() {
            transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        }
        
        [Client]
        public void ResetCardScale() {
            transform.localScale = Vector3.one;
        }
        
        [Client]
        public void ResetCard() {
            ResetCardScale();
            GlowCard(false);
        }
        
        [Client]
        public void OnPointerClick(PointerEventData eventData) {
            _currentState.OnMouseClick(this, eventData);        
        }
        
        [Client]
        public void OnPointerExit(PointerEventData eventData) {
            _currentState.OnMouseExit(this, eventData);
        }
        
        [Client]
        public void OnBeginDrag(PointerEventData eventData) {
            _currentState.OnBeginDrag(this, eventData);
        }
        
        [Client]
        public void OnDrag(PointerEventData eventData) {
            _currentState.OnMouseDrag(this, eventData);
        }
        
        [Client]
        public void OnEndDrag(PointerEventData eventData) {
            _currentState.OnEndDrag(this, eventData);
        }
        
        [Client]
        public void OnPointerEnter(PointerEventData eventData) {
            _currentState.OnMouseHover(this, eventData);
        }
        
        [Client]
        private void OnEnable() {
            if (!isUsedInBattle || !Card.CardStruct.isSelfDestroyed) return;
            RemoveOccupiedJointCard();
            SelfDestroy();
        }

        [Client]
        public void DrawCard() {
            // Debug.LogWarning($"DrawCard is called");
            var jointCardCount = _jointCardUITransform.childCount;
            for (int i = 0; i < jointCardCount; i++) {
                var jointCardObject = _jointCardUITransform.GetChild(i).gameObject;
                var jointCard = jointCardObject.GetComponent<NwJointCard>();
                var isSameType = IsSameCardType(jointCardObject);
                if (!isSameType || jointCard.OccupiedCard != null) continue;
                var isEnoughEnergy = TurnManager.Instance.SubtractEnergy(Card.CardStruct.energy);
                if (!isEnoughEnergy) continue;
                SetParent(_playerCardUITransform);
                SetOccupiedJointCard(jointCard);
                CmdUpdateCardStatus(true);
                CardDeckManager.Instance.CmdAddDrawnCard(netId, _playerId);
                SwitchState(_selectedState);
                break;
            }
            
        }

        [Client]
        public bool IsSameCardType(GameObject jointCard) {
            //Debug.LogWarning($"{Card.CardStruct.jointCardType} : {jointCard.name}");
            return (Card.CardStruct.jointCardType == jointCard.name);
        }
        
        [Client]
        public void ReturnCardToPileCard() {
            RemoveOccupiedJointCard();
            CmdUpdateCardStatus(false);
            CmdShowCard();
            SetParent(_cardPileUITransform);
            _currentState = _drawableState;
            TurnManager.Instance.AddEnergy(Card.CardStruct.energy);
            CardDeckManager.Instance.CmdRemoveDrawnCard(netId);
        }
        
        [Client]
        private void SetParent(Transform parent) {
            transform.SetParent(parent, false);
        }

        [Client]
        public void SetPositionTo(Vector3 newPosition) {
            //newPosition.z -= 1f;
            transform.position = newPosition;
        }
        
        [Client]
        public void SetOccupiedJointCard(NwJointCard jointCard) {
            SetPositionTo(jointCard.Position);
            OccupiedJointCard = jointCard;
            jointCard.SetOccupiedCard(Card);
            RequestAttachComponentOnChassis();
            CmdHideCardOnOpponentSide();
        }

        [Command(requiresAuthority = true)]
        private void CmdUpdateCardStatus(bool drawn) {
            isDrawn = drawn;
        }
        
        [Command(requiresAuthority = true)]
        private void CmdHideCardOnOpponentSide() {
            RpcHideCard(false);
        }
        
        [Command(requiresAuthority = true)]
        private void CmdShowCard() {
            RpcHideCard(true);
        }
        
        [ClientRpc]
        private void RpcHideCard(bool shown) {
            if (!hasAuthority) {
                gameObject.transform.SetAsLastSibling();
                gameObject.SetActive(shown);
            }
                
        }
        
        [Client]
        public void DetachComponentObjectFromChassis() {
            RequestDetachComponentOnChassis();
        }
        
        [Client]
        public void RemoveOccupiedJointCard() {
            if (OccupiedJointCard == null) return;
            OccupiedJointCard.SetOccupiedCard(null);
            OccupiedJointCard = null;
        }
        
        [Client]
        private void RequestAttachComponentOnChassis() {
            var componentPath = Card.CardStruct.robotComponentPrefabPath;
            var jointId = OccupiedJointCard.ChassisJoint.jointId;
            var damage = Card.CardStruct.damage;
            var health = Card.CardStruct.health;
            _playerManager.CmdAttachComponentOnChassis(jointId, componentPath, damage, health);
        }
        
        [Client]
        private void RequestDetachComponentOnChassis() {
            var jointId = OccupiedJointCard.ChassisJoint.jointId;
            var damage = Card.CardStruct.damage;
            var health = Card.CardStruct.health;
            _playerManager.CmdDetachComponentOnChassis(jointId, damage, health);
        }

        public void SelfDestroy() {
            CardDeckManager.Instance.CmdRemoveDrawnCard(netId);
            Destroy(gameObject);
        }
    }
}