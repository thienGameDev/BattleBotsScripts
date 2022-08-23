using UnityEngine.EventSystems;

namespace CardStates {
    public abstract class CardBaseState {
        public abstract void EnterState(CardStateManager card);
        public abstract void OnMouseHover(CardStateManager card, PointerEventData eventData);
        public abstract void OnMouseExit(CardStateManager card, PointerEventData eventData);
        public abstract void OnMouseClick(CardStateManager card, PointerEventData eventData);
        public abstract void OnBeginDrag(CardStateManager card, PointerEventData eventData);
        public abstract void OnMouseDrag(CardStateManager card, PointerEventData eventData);
        public abstract void OnEndDrag(CardStateManager card, PointerEventData eventData);
    }
}
