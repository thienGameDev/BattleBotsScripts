using ManagerLib;
using UnityEngine;

namespace ComponentStates.ChassisStates {
    public class ChassisStandbyState : ChassisBaseState {
        private const string WALL_TAG = "Wall";
        private const string GROUND_TAG = "Ground";
        private string _eventChassisGrounded;
        private const int FORCE_FACTOR = 5000;
        public override void EnterState(ChassisStateManager chassis) {
            chassis.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            _eventChassisGrounded = GROUND_TAG + chassis.gameObject.GetInstanceID();
            chassis.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.down * FORCE_FACTOR, ForceMode2D.Force);
        }

        private void RemoveGrapplingRope(ChassisStateManager chassis) {
            chassis.grapplingLauncher.RemoveGrappling();
            chassis.nwChassis.Grappling(false);
        }

        public override void UpdateState(ChassisStateManager chassis) {

        }

        public override void OnCollisionEnter2D(ChassisStateManager chassis, Collision2D other) {
            if (other.gameObject.CompareTag(WALL_TAG)) {
                var eventHitTheWall = WALL_TAG + chassis.gameObject.GetInstanceID();
                var wallPosX = other.transform.position.x;
                EventManager.TriggerEvent(eventHitTheWall, wallPosX);
                Debug.Log($"Hit The Wall, Event {eventHitTheWall}");
            }

            if (other.gameObject.CompareTag(GROUND_TAG)) {
                EventManager.TriggerEvent(_eventChassisGrounded, 0f);
                Debug.Log($"Chassis {chassis.gameObject.GetInstanceID()} Hit the ground");
            }
        }
    }
}