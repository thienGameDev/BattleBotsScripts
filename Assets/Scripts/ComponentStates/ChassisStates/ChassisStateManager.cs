using EnumTypes;
using Interfaces;
using ManagerLib;
using RobotComponentLib.ChassisLib;
using RobotComponentLib.ChassisLib.GrapplingRopeLib;
using UnityEngine;

namespace ComponentStates.ChassisStates {
    public class ChassisStateManager : ComponentStateManager, IFreezable  {
        private ChassisBaseState _currentState;
        private ChassisBaseState _idleState = new ChassisIdleState();
        private ChassisBaseState _standbyState = new ChassisStandbyState();
        // public ChassisBaseState OnDamagedState = new ChassisOnDamagedState();
        // public ChassisBaseState ExplodingState = new ChassisExplodingState();
        public NwChassis nwChassis;
        public Vector2 grapplePoint;
        public GrapplingLauncher grapplingLauncher;
        
        private void Awake() {
            nwChassis = gameObject.GetComponent<NwChassis>();
            grapplingLauncher = gameObject.GetComponentInChildren<GrapplingLauncher>();
        }

        private void Start() {
            _currentState = _idleState;
            _currentState.EnterState(this);
            //DontDestroyOnLoad(gameObject);
            EventManager.StartListening(GameEvent.EndRound.ToString(), ChassisOnEndRound);
        }

        private void ChassisOnEndRound(float arg0) {
            Freeze();
        }
        
        private void OnCollisionEnter2D(Collision2D other) {
            _currentState?.OnCollisionEnter2D(this, other);
        }
        
        // Update is called once per frame
        private void Update()
        {
            _currentState.UpdateState(this);
        }

        private void SwitchState(ChassisBaseState state) {
            _currentState = state;
            state.EnterState(this);
        }
        
        public void Freeze() {
            var childCount= transform.childCount;
           // Debug.Log("Transform Length: " + childCount);
            for (var i = 0; i < childCount; i++) {
                GameObject robotComponent = transform.GetChild(i).gameObject;
         //       Debug.Log("Object: " + robotComponent.name);
                var freezableComponent = robotComponent.GetComponent<IFreezable>();
                freezableComponent?.Freeze();
            }
            SwitchState(_idleState);
        }

        public void Unfreeze() {
            var childCount= transform.childCount;
            //Debug.Log("Transform Length: " + childCount);
            for (var i = 0; i < childCount; i++) {
                GameObject robotComponent = transform.GetChild(i).gameObject;
                //Debug.Log("Object: " + robotComponent.name);
                var freezableComponent = robotComponent.GetComponent<IFreezable>();
                freezableComponent?.Unfreeze();
            }
            SwitchState(_standbyState);
        }
    }
}
