using Interfaces;
using ManagerLib;
using UnityEngine;

namespace ComponentStates.BoosterStates {
    public class BoosterStateManager : ComponentStateManager, IFreezable
    {
        public float loadingBoosterTime = 1f;
        public float boostingForce = 210f;
        private const string GROUND_TAG = "Ground";
        private BoosterBaseState _currentState;
        private BoosterBaseState _idleState = new BoosterIdleState();
        private BoosterBaseState _boostingState = new BoosterBoostingState();
        public BoosterBaseState endState = new BoosterEndState();
        private string _eventChassisGrounded;

        private void OnEnable() {
            if (Head != null && Tail != null) return;
            CreateHeadTail();
        }
        private void Start() {
            GameObject attachedChassis = gameObject.transform.parent.gameObject;
            _eventChassisGrounded = GROUND_TAG + attachedChassis.GetInstanceID();
            EventManager.StartListening(_eventChassisGrounded, StartBoosting);
            _currentState = _idleState;
            _currentState.EnterState(this);
        }

        private void StartBoosting(float arg0) {
            EventManager.StopListening(_eventChassisGrounded, StartBoosting);
            SwitchState(_boostingState);
        }

        // Update is called once per frame
        private void Update()
        {
            _currentState.UpdateState(this);
        }

        public void SwitchState(BoosterBaseState state) {
            _currentState = state;
            state.EnterState(this);
        }

        public void Freeze() {
            //SwitchState(_idleState);
        }

        public void Unfreeze() {
            SwitchState(_boostingState);
        }

        private void CreateHeadTail() {
            var head = new GameObject("Head") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.right
                }
            };
            Head = head.transform;
            var tail = new GameObject("Tail") {
                transform = {
                    parent = transform,
                    localPosition = Vector3.left
                }
            };
            Tail = tail.transform;
        }
    }
}
