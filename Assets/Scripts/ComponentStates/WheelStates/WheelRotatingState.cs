using ManagerLib;
using UnityEngine;

namespace ComponentStates.WheelStates {
    public class WheelRotatingState : WheelBaseState {
        private const string WALL_TAG = "Wall";
        private const string GROUND_TAG = "Ground";
        private const float MAX_FREQUENCY = 15f;
        private const float MIN_FREQUENCY = 3f;
        private const float MAX_DAMPING = 0.5f;
        private const float MIN_DAMPING = 0.1f;
        private const float MAX_CHASSIS_Y = 3.5f;
        private static float _groundY;
        private string _eventChassisGrounded;
        private WheelStateManager _wheel;
        
        public override void EnterState(WheelStateManager wheel) {
            var rigidBody = wheel.gameObject.GetComponent<Rigidbody2D>();
            rigidBody.bodyType = RigidbodyType2D.Dynamic;
            _wheel = wheel;
            var wheelJoint = _wheel.gameObject.GetComponent<WheelJoint2D>();
            wheelJoint.useMotor = true;
            _eventChassisGrounded = GROUND_TAG + wheel.transform.root.gameObject.GetInstanceID();
        }

        public override void UpdateState(WheelStateManager wheel) {
            // WheelSuspensionAdapt();
        }

        private void WheelSuspensionAdapt() {
            var deltaY = CalculateDeltaY();
            //Debug.Log($"DeltaY: {deltaY}");
            var wheelJoint2D = _wheel.gameObject.GetComponent<WheelJoint2D>();
            var wheelSuspension = new JointSuspension2D {
                dampingRatio = Mathf.Lerp(MAX_DAMPING,MIN_DAMPING, deltaY/MAX_CHASSIS_Y),
                frequency = Mathf.Lerp(MAX_FREQUENCY,MIN_FREQUENCY, deltaY/MAX_CHASSIS_Y)
            };
            wheelJoint2D.suspension = wheelSuspension;
        }

        private float CalculateDeltaY() {
            var chassisY = _wheel.transform.root.position.y;
            return Mathf.Abs(chassisY - _groundY); 
        }
        public override void OnCollisionEnter2D(WheelStateManager wheel, Collision2D other) {
            if (other.gameObject.CompareTag(WALL_TAG)) {
                var parentId = wheel.gameObject.transform.parent.gameObject.GetInstanceID().ToString();
                var eventChassisHitWall = WALL_TAG + parentId;
                var wallPosX = other.transform.position.x;
                EventManager.TriggerEvent(eventChassisHitWall, wallPosX);
            }

            if (other.gameObject.CompareTag(GROUND_TAG)) {
                _groundY = _wheel.transform.root.position.y;
                EventManager.TriggerEvent(_eventChassisGrounded, 0f);
            }
        }

        public override void OnCollisionExit2D(WheelStateManager wheel, Collision2D other) {
            // if (other.gameObject.CompareTag("Ground")) {
            //     wheel.SwitchState(wheel.OnAirState);
            // }
        }
    }
}