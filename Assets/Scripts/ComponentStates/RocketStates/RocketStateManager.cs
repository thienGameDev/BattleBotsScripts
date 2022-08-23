using Mirror;
using UnityEngine;

namespace ComponentStates.RocketStates {
    public class RocketStateManager : RangedWeaponChildStateManager {
        private RocketBaseState _currentState;
        private RocketBaseState _idleState = new RocketIdleState();
        private RocketBaseState _launchingState = new RocketLaunchingState();
        private RocketBaseState _explodingState = new RocketExplodingState();

        public override void OnStartServer() {
            base.OnStartServer();
            _currentState = _idleState;
            _currentState.EnterState(this);
        }
        
        [Command(requiresAuthority = false)]
        public void CmdSwitchToExplodingState() {
            // Debug.LogWarning($"CmdSwitchToExplodingState is called by {netId}");
            SwitchState(_explodingState);        
        }
        
        private void OnTriggerEnter2D(Collider2D other) {
            if (isClientOnly) {
                _currentState = _launchingState;
                // Debug.Log($"Client enter state {_currentState}");
                _currentState.OnClientTriggerEnter2D(this, other);
            }
            else if (isServer) {
                _currentState.OnServerTriggerEnter2D(this, other);
            }
        }
        
        // Update is called once per frame
        private void Update()
        {
            if(isClientOnly) return;
            _currentState?.UpdateState(this);
        }
        
        [Server]
        private void SwitchState(RocketBaseState newState) {
            _currentState = newState;
            _currentState.EnterState(this);
        }
        
        public override void EnterOnHold() {
            SwitchState(_idleState);
        }
        
        public override void Execute() {
            SwitchState(_launchingState);
        }
        
    }
}
