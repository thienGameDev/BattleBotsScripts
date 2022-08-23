using UnityEngine.EventSystems;

namespace CardStates {
    public class CardUndrawableState : CardBaseState {
        public override void EnterState(CardStateManager card) {
            card.ResetCardScale();
            card.GlowCard(false);
        }
        
        public override void OnMouseHover(CardStateManager card, PointerEventData eventData) {
            
        }

        public override void OnMouseExit(CardStateManager card, PointerEventData eventData) {
            
        }
        
        public override void OnMouseClick(CardStateManager card, PointerEventData eventData) {
            
        }

        public override void OnBeginDrag(CardStateManager card, PointerEventData eventData) {
            
        }

        public override void OnMouseDrag(CardStateManager card, PointerEventData eventData) {
            
        }

        public override void OnEndDrag(CardStateManager card, PointerEventData eventData) {
            
        }
        
    }
}