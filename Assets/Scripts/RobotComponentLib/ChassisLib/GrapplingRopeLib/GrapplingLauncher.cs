using UnityEngine;

namespace RobotComponentLib.ChassisLib.GrapplingRopeLib {
    public class GrapplingLauncher : MonoBehaviour {
        private GrapplingRope _grappleRope;
        
        [Header("Spring Joint:")]
        [SerializeField] private float targetDistance = 3;
        [SerializeField] private float targetFrequency = 1;
        [SerializeField] private float offsetY = 0.1f;
        
        [HideInInspector] public Vector2 grapplePoint;
        [HideInInspector] public Transform firePoint;
        [HideInInspector] public Vector2 grappleDistanceVector;
        private Vector2 _centerOfMass;
        private SpringJoint2D _springJoint2D;
        private Rigidbody2D _rigidBody2D;
        
        private void Awake() {
            _grappleRope = GetComponentInChildren<GrapplingRope>();
            _springJoint2D = GetComponentInParent<SpringJoint2D>();
            _rigidBody2D = GetComponentInParent<Rigidbody2D>();
            _grappleRope.enabled = false;
            _springJoint2D.enabled = false;
            _centerOfMass = _rigidBody2D.centerOfMass;
            _centerOfMass.y += offsetY;
            firePoint = transform;
            firePoint.position = transform.TransformPoint(_centerOfMass);
            // Debug.Log($"Center of mass: {firePoint.position}");
        }
        
        public void StartGrappling(Vector2 position) {
            if(_grappleRope.enabled) return;
            grapplePoint = position;
            grappleDistanceVector = grapplePoint - (Vector2)transform.position;
            // Debug.Log($"GrapplePoint: {grapplePoint}");
            _grappleRope.enabled = true;
        }

        public void RemoveGrappling() {
            if (!_grappleRope.enabled) return;
                _grappleRope.enabled = false;
                _springJoint2D.enabled = false;
        }
        
        public void Grapple() {
            _springJoint2D.autoConfigureDistance = false;
            _springJoint2D.distance = targetDistance;
            _springJoint2D.frequency = targetFrequency;
            _springJoint2D.connectedAnchor = grapplePoint;
            _springJoint2D.anchor = _centerOfMass;
            _springJoint2D.enabled = true;
        }
    }
}