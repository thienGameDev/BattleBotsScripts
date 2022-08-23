using Mirror;

namespace UI {
    public class BattleView : NetworkBehaviour {
        public override void OnStartClient() {
            base.OnStartClient();
            if(isClientOnly) FlipBattleView();
        }
        
        [Client]
        private void FlipBattleView() {
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}