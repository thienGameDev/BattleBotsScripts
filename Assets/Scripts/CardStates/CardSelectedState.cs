using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CardStates {
    public class CardSelectedState : CardBaseState
    {
        private Vector3 _dragOffset;
        private Collider2D _collider2D;
        private NwJointCard _closestJointCard;
        private Transform _thisCardTransform;
        private ContactFilter2D _jointFilter2D;
        private bool _isDragged;
        private RectTransform _thisRectTransform;

        public override void EnterState(CardStateManager card) {
            // Debug.LogWarning($"{card.Card.CardStruct.cardName} entered selected state");
            _thisCardTransform = card.transform;
            _thisRectTransform = card.gameObject.GetComponent<RectTransform>();
            _collider2D = card.gameObject.GetComponent<Collider2D>();
            _jointFilter2D = new ContactFilter2D();
            _jointFilter2D.SetLayerMask(LayerMask.GetMask("JointCardUI"));
        }
        
        public override void OnMouseHover(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"SelectedState - OnMouseOver is executed");
        }

        public override void OnMouseExit(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"SelectedState - MouseExit is executed");
        }

        public override void OnBeginDrag(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"SelectedState - BeginDrag is executed");
            Vector3 mousePos = eventData.position;
            _dragOffset = _thisRectTransform.localPosition - mousePos;
            card.DetachComponentObjectFromChassis();
            card.transform.SetAsLastSibling();
            _isDragged = true;
        }

        public override void OnMouseDrag(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"SelectedState - OnMouseDrag is executed");
            Vector3 mousePos = eventData.position;
            _thisRectTransform.localPosition = mousePos + _dragOffset;
            GetClosestJointCard();
        }

        public override void OnEndDrag(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"SelectedState - EndDrag is executed");
            if (_closestJointCard != null) {
                CardTransferHandler(card);
            }
            else {
                card.ReturnCardToPileCard();
            }
            _isDragged = false;
        }
        
        public override void OnMouseClick(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"SelectedState - OnMouseClick is executed");
            if(_isDragged) return;
            card.DetachComponentObjectFromChassis();
            if (card.isUsedInBattle) card.SelfDestroy();
            else card.ReturnCardToPileCard();
        }

        private void CardTransferHandler(CardStateManager card) {
            var isSameCardType = card.IsSameCardType(_closestJointCard.gameObject);
            if (isSameCardType & card.OccupiedJointCard != _closestJointCard) {
                if (_closestJointCard.OccupiedCard != null) SwapCard(card);
                else TransferCardToNewPosition(card);
            }
            else {
                if (card.isUsedInBattle) card.SelfDestroy();
                else card.ReturnCardToPileCard();
            }
        }

        private void SwapCard(CardStateManager card) {
            var thisJointCard = card.OccupiedJointCard;
            //start swapping this card with closest card
            var closestCard = _closestJointCard.OccupiedCard.gameObject.GetComponent<CardStateManager>();
            // Detach current component of closest Card
            closestCard.DetachComponentObjectFromChassis();
            // Moving closest Card toward this JointCard
            closestCard.SetOccupiedJointCard(thisJointCard);
            // Moving this card to closest joint card
            card.SetOccupiedJointCard(_closestJointCard);
        }

        private void TransferCardToNewPosition(CardStateManager card) {
            card.RemoveOccupiedJointCard();
            card.SetOccupiedJointCard(_closestJointCard);
        }
        
        private void GetClosestJointCard() {
            List<Collider2D> contactColliders = new List<Collider2D>();
            var nearbyObjects = _collider2D.OverlapCollider(_jointFilter2D, contactColliders);
            Collider2D closestJointCardCollider;
            switch (nearbyObjects) {
                case 0:
                    closestJointCardCollider = null;
                    break;
                case 1:
                    closestJointCardCollider = contactColliders[0];
                    break;
                default:
                    closestJointCardCollider = GetClosestColliderObject(contactColliders);
                    break;
            }
            _closestJointCard = closestJointCardCollider != null ? closestJointCardCollider.gameObject.GetComponent<NwJointCard>() : null;

            if (_closestJointCard != null) Debug.Log($"_closest JointCard: {_closestJointCard.ChassisJoint.jointId}");
        }
        private Collider2D GetClosestColliderObject(List<Collider2D> contactColliders) {
            var closestDistance = float.MaxValue;
            var closestCollider2D = new Collider2D();
            foreach (var colliderObject in contactColliders) {
                Vector3 colliderObjectPos = colliderObject.transform.position;
                var distance = Vector2.Distance(_thisCardTransform.position, colliderObjectPos);
                if (distance < closestDistance) { 
                    closestDistance = distance;
                    closestCollider2D = colliderObject;
                }
            }
            return closestCollider2D;
        }
    }
}
