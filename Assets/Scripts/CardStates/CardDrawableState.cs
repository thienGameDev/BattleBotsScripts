using UnityEngine.EventSystems;

namespace CardStates {
    public class CardDrawableState : CardBaseState {
        public override void EnterState(CardStateManager card) {

        }
        
        public override void OnMouseHover(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"DrawableState - MouseOver is executed");
            card.ScaleCard();
            card.GlowCard(true);
        }

        public override void OnMouseExit(CardStateManager card, PointerEventData eventData) {
            card.ResetCard();
            // Debug.Log($"DrawableState - MouseExit is executed");
        }
        
        public override void OnBeginDrag(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"DrawableState - OnBeginDrag is executed");
        }

        public override void OnMouseDrag(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"DrawableState - MouseDrag is executed");
        }

        public override void OnEndDrag(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"DrawableState - EndDrag is executed");
        }
        
        public override void OnMouseClick(CardStateManager card, PointerEventData eventData) {
            // Debug.Log($"DrawableState - OnMouseClick is executed");
            card.DrawCard();
        }
    }
}
