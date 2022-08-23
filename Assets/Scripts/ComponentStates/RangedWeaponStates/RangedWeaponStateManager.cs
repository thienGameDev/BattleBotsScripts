using Interfaces;
using RobotComponentLib.RangedWeaponLib;
using UnityEngine;

namespace ComponentStates.RangedWeaponStates {
    public class RangedWeaponStateManager : ComponentStateManager, IFreezable {
        [HideInInspector] public NwRangedWeapon nwRangedWeapon;
        private RangedWeaponBaseState _staticState = new RangedWeaponStaticState();
        private RangedWeaponBaseState _dynamicState = new RangedWeaponDynamicState();
        private RangedWeaponBaseState _currentState;

        private void Awake() {
            nwRangedWeapon = GetComponent<NwRangedWeapon>();
        }

        private void Start() {
            _currentState = _staticState;
            _currentState.EnterState(this);
        }

        private void Update() {
            _currentState.UpdateState(this);
        }

        private void SwitchState(RangedWeaponBaseState state){
            _currentState = state;
            state.EnterState(this);
        }

        public void Freeze() {
            SwitchState(_staticState);
        }

        public void Unfreeze() {
            SwitchState(_dynamicState);
        }
    }
}