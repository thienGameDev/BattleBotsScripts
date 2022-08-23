using ComponentStates;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RobotComponentLib {
    public abstract class NwRobotComponent : NetworkBehaviour {
        public bool isSelfDestroyable;
        [SyncVar] public uint parentNetId;
        [SyncVar] public Vector3 localPosition;
        [SyncVar] public bool isFlipped;
        protected const float DAMAGE_BY_OBSTACLE = 5f;
        public float Damage {
            get => _damage - _dmgOffset;
            set {
                while (_dmgOffset == 0) {
                    _dmgOffset = Random.Range(-10000, 10001);
                    // Debug.LogWarning($"Init _dmgOffset: {_dmgOffset}");
                }
                _damage = value + _dmgOffset;
                // Debug.LogWarning($"Get _damage: {_damage} - _dmgOffset: {_dmgOffset}");
            }
        }

        private float _damage;
        private int _dmgOffset;
        
        public override void OnStartServer() {
            base.OnStartServer();
            OnSpawnServer();
        }

        public override void OnStartClient() {
            base.OnStartClient();
            OnSpawnClient();
        }
        
        [Server]
        protected virtual void OnSpawnServer() {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        [Command(requiresAuthority = false)]
        protected void CmdSetRobotComponentDynamic() {
            // Debug.Log($"CmdSetRobotComponentDynamic is called");
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
        
        [Client]
        protected virtual void OnSpawnClient() {
            if (!isClientOnly || gameObject.CompareTag("Chassis")) return;
            Remove<ComponentStateManager>(gameObject);
            Remove<Joint2D>(gameObject);
            Remove<Rigidbody2D>(gameObject);
            if(NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity networkIdentity)) {
                var parent = networkIdentity.gameObject;
                transform.parent = parent.transform;
                transform.rotation = parent.transform.rotation;
                //transform.position = localPosition;
                transform.localPosition = localPosition;
                //parent.GetComponent<NwChassis>()?.CmdSetRobotComponentDynamic();
                Debug.Log($"Parent is set");
            }
            else {
                Debug.Log($"Not found netId {parentNetId}. NetworkClient.Count: {NetworkClient.spawned.Count}");
            }
            if(!hasAuthority) FlipObject();
            CmdSetRobotComponentDynamic();
        }
        
        public virtual void FlipObject() {
            var localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
            // Debug.LogWarning($"Object {netId} is flipped {transform.lossyScale}");
        }
        
        public virtual void MirrorObject() {
            var position = transform.localPosition;
            position.x *= -1;
            transform.localPosition = position;
            // Debug.Log("Object is mirrored on y-Axis");
        }
        
        public virtual void ReverseDirection() {
            var wheelJoint = GetComponent<WheelJoint2D>();
            if(!wheelJoint) return;
            var usedMotor = wheelJoint.useMotor;
            JointMotor2D motorJoint = new JointMotor2D {
                motorSpeed = wheelJoint.motor.motorSpeed * -1,
                maxMotorTorque = wheelJoint.motor.maxMotorTorque
            };
            GetComponent<WheelJoint2D>().motor = motorJoint;
            GetComponent<WheelJoint2D>().useMotor = usedMotor;
        }
        
        [Client]
        protected void Remove<T>(GameObject obj) where T: Component {
            if(obj.TryGetComponent<T>(out var component)) Destroy(component);
        }
    }
}