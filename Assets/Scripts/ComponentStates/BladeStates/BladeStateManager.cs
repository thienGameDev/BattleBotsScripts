using Interfaces;
using UnityEngine;

namespace ComponentStates.BladeStates {
    public class BladeStateManager : ComponentStateManager, IFreezable {
        private BladeBaseState _currentState;
        private BladeBaseState _lastState;
        private BladeBaseState _idleState = new BladeIdleState();
        private BladeBaseState _operatingState = new BladeOperatingState();
        
        // Start is called before the first frame update
        private void Start()
        {
            _currentState = _idleState;
            _currentState.EnterState(this);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            _currentState?.OnTriggerEnter2D(this, other);
        }
        
        // Update is called once per frame
        private void Update()
        {
            _currentState.UpdateState(this);
        }

        private void SwitchState(BladeBaseState state) {
            _lastState = _currentState;
            _currentState = state;
            state.EnterState(this);
        }

        public void Freeze() {
            SwitchState(_idleState);
        }

        public void Unfreeze() {
            SwitchState(_lastState ?? _operatingState);
        }
    }
}