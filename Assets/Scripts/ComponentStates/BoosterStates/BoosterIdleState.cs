namespace ComponentStates.BoosterStates {
    public class BoosterIdleState : BoosterBaseState
    {
        public override void EnterState(BoosterStateManager booster) {
           // booster.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }   
        public override void UpdateState(BoosterStateManager booster){
        
        }
    }
}
