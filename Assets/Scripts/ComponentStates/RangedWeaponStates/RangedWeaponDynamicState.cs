namespace ComponentStates.RangedWeaponStates {
    public class RangedWeaponDynamicState : RangedWeaponBaseState {
        public override void EnterState(RangedWeaponStateManager rangedWeapon) {
            rangedWeapon.nwRangedWeapon.StartFiring();
        }
        public override void UpdateState(RangedWeaponStateManager rangedWeapon) {

        }
        
    }
}