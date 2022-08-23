using Interfaces;
using UnityEngine;

namespace ComponentStates.WheelStates {
    public class WheelStateManager : ComponentStateManager, IFreezable {
        private WheelBaseState _currentState;
        private WheelBaseState _idleState = new WheelIdleState();
        private WheelBaseState _rotatingState = new WheelRotatingState();

        // Start is called before the first frame update
        private void Start()
        {
            _currentState = _idleState;
            _currentState.EnterState(this);
        }

        private void OnCollisionEnter2D(Collision2D other) {
            _currentState?.OnCollisionEnter2D(this, other);
        }
        
        private void OnCollisionExit2D(Collision2D other) {
            _currentState?.OnCollisionExit2D(this, other);
        }
        
        // Update is called once per frame
        private void Update()
        {
            _currentState.UpdateState(this);
        }

        private void SwitchState(WheelBaseState state){
            _currentState = state;
            state.EnterState(this);
        }

        public void Freeze() {
            SwitchState(_idleState);
        }

        public void Unfreeze() {
            SwitchState(_rotatingState);
        }
    }
}