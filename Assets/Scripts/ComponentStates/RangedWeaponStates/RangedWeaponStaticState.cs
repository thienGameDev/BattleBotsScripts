namespace ComponentStates.RangedWeaponStates {
    public class RangedWeaponStaticState : RangedWeaponBaseState {
        public override void EnterState(RangedWeaponStateManager rangedWeapon) {
            rangedWeapon.nwRangedWeapon.StopFiring();
        }

        public override void UpdateState(RangedWeaponStateManager rangedWeapon) {
            
        }
    }
}