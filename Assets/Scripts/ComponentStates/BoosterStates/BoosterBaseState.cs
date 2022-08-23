namespace ComponentStates.BoosterStates {
    public abstract class BoosterBaseState 
    {
        public abstract void EnterState(BoosterStateManager booster);
        public abstract void UpdateState(BoosterStateManager booster);
    }
}
