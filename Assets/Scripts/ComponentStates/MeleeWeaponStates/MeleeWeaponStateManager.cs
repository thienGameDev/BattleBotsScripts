using Interfaces;

namespace ComponentStates.MeleeWeaponStates {
    public class MeleeWeaponStateManager : ComponentStateManager, IFreezable {
        private MeleeWeaponBaseState _currentState;
        private MeleeWeaponBaseState _staticState = new MeleeWeaponStaticState();
        private MeleeWeaponBaseState _dynamicState = new MeleeWeaponDynamicState();

        private void Start() {
            _currentState = _staticState;
            _currentState.EnterState(this);
        }

        private void Update() {
            _currentState.UpdateState(this);
        }

        private void SwitchState(MeleeWeaponBaseState newState) {
            _currentState = newState;
            _currentState.EnterState(this);
        }

        public void Freeze() {
            SwitchState(_staticState);
        }

        public void Unfreeze() {
            SwitchState(_dynamicState);
        }
    }
}