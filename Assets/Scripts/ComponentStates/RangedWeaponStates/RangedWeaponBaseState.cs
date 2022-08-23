namespace ComponentStates.RangedWeaponStates {
    public abstract class RangedWeaponBaseState {
        public abstract void EnterState(RangedWeaponStateManager rangedWeapon);
        public abstract void UpdateState(RangedWeaponStateManager rangedWeapon);
    }
}