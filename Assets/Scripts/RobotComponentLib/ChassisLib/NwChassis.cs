using System;
using System.Collections.Generic;
using System.Linq;
using ComponentStates.ChassisStates;
using ManagerLib;
using Mirror;
using RobotComponentLib.ChassisLib.GrapplingRopeLib;
using RobotComponentLib.WheelLib;
using UI;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace RobotComponentLib.ChassisLib {
    public class NwChassis : NwRobotComponent {
        public readonly SyncDictionary<byte, ChassisJoint> jointDict = new();
        private readonly SyncDictionary<byte, uint> _jointComponentDict = new ();
        [SyncVar(hook = nameof(OnGrappledChange))] private bool _grappled;
        public Vector2 GrapplePoint {
            set => _grapplePoint = value;
        }
        private Vector2 _grapplePoint;
        private Vector2 _grapplePointLeft;
        private Vector2 _grapplePointRight;
        private List<GameObject> _jointList;
        private Bounds _chassisBound;
        private float _minY, _maxY, _minX, _maxX;
        private float _maxWheelJointY;
        private PolygonCollider2D _chassisCollider2D;
        private List<Vector2> _colliderMeshPoints;
        private int _jointCount;
        private float _lowerLimit;
        private float _heightQuotient;
        private int _colliderMeshVerticalCount;
        private string _eventChassisHitTheWall;
        private string _instanceID;
        private Rigidbody2D _rigidBody2d;
        private GrapplingRope _grapplingRope;
        private GrapplingLauncher _grapplingLauncher;

        private const string JOINT_CARD_PREFAB_PATH = "Prefabs/UI/JointCard";
        private const string WALL_TAG = "Wall";
        private const string JOINT_NAME = "Joint";
        private const float MESH_SIZE_X = 0.1f;
        private const float MESH_SIZE_Y = 0.1f;
        private const float MIN_WHEEL_DISTANCE = 0.2f;
        private const int HEIGHT_DIVISOR = 6;
        private const int MAX_JOINT_SLOTS = 5;
        private bool _quit;
        
        // Debug:
        private const string MESH_POINT_PATH = "Prefabs/RobotComponent/JointHolder/MeshPoint";
        private GameObject _meshPoint;
        
        
        private void Awake() {
            try {
                //_grapplePointLeft = GameObject.FindGameObjectWithTag("GrapplePointLeft").transform.position;
                //_grapplePointRight = GameObject.FindGameObjectWithTag("GrapplePointRight").transform.position;
                //_grapplingLauncher = GetComponentInChildren<GrapplingLauncher>();
                //_grapplingRope = GetComponentInChildren<GrapplingRope>();
            }
            catch (Exception e) {
                // ignore
                // Debug.LogWarning($"GrapplePoints or GrapplingLaunchers not found. Error: {e}");
            }
        }

        private void OnEnable() {
            _jointList = new List<GameObject>();
            GetJointList();
        }

        public struct ChassisJoint {
            public byte jointId;
            public byte typeId;
            public string type;
            public Vector3 position;
        }
        
        #region Server
        
        public void InitializeChassis(List<Vector3> jointPositionList) {
            byte id = 0;
            foreach (var jointPosition in jointPositionList) {
                var joint = new ChassisJoint() {
                    jointId = id,
                    typeId = (byte)jointPosition.z,
                    position = new Vector3() {
                        x = jointPosition.x,
                        y = jointPosition.y
                    },
                    type = _jointList[(int)jointPosition.z].tag.Split(" ")[0]
                };
                jointDict.Add(id, joint);
                id++;
            }
            // Health = health;
        }
        
        [Server]
        protected override void OnSpawnServer() {
            base.OnSpawnServer();
            _instanceID = gameObject.GetInstanceID().ToString();
            _rigidBody2d = GetComponent<Rigidbody2D>();
            //JointSetup();
            SetJointsActive();
            RegisterServerEvent();
            // Debug.Log($"Instance Id on Server: {GetInstanceID()}");
        }
        
        [Command(requiresAuthority = true)]
        private void CmdStartGrappling() {
            Grappling(true);
        }

        public void Grappling(bool grappled) {
            _grappled = grappled;
        }

        [Command(requiresAuthority = true)]
        private void CmdInitChassisStateManager() {
            var chassisManager = GetComponent<ChassisStateManager>();
            //chassisManager.grapplePoint = _grapplePoint;
            chassisManager.enabled = true;
        }

        #region EventHandlers

        [Server]
        private void RegisterServerEvent() {
            _eventChassisHitTheWall = CreateEventName(WALL_TAG);
            EventManager.StartListening(_eventChassisHitTheWall, OnChassisHitTheWall);
        }

        [Server]
        private void OnChassisHitTheWall(float wallPosX) {
            // Go backward
            Debug.Log("OnChassisHitTheWall function called " + wallPosX);
            var attachedWheelIds =
                _jointComponentDict
                    .Where(joint => joint.Key < 2)
                    .Select(joint => joint.Value).ToList();
            if(attachedWheelIds.Count == 0) return;
            var attachedWheels = attachedWheelIds.Select(GetSpawnedObject).ToList();
            var currentWheelSpeed = attachedWheels[0].GetComponent<WheelJoint2D>().motor.motorSpeed;
            if ((wallPosX > 0 && currentWheelSpeed > 0) 
                || (wallPosX < 0 && currentWheelSpeed < 0)) {
                foreach (var wheel in attachedWheels) {
                    wheel.GetComponent<NwWheel>().ReverseDirection();
                }
            }
        }
        
        [ServerCallback]
        private void OnBecameInvisible() {
            if(!_quit)
                TakeDamageByObstacles(float.MaxValue);
        }

        [ServerCallback]
        private void OnCollisionEnter2D(Collision2D col) {
            if (col.gameObject.CompareTag("Obstacle")) {
                Debug.LogWarning($"Hit obstacle: {col.gameObject.name}");
                TakeDamageByObstacles(DAMAGE_BY_OBSTACLE);
            }
        }

        [ServerCallback]
        private void OnCollisionStay2D(Collision2D collision) {
            if (collision.gameObject.CompareTag("Obstacle")) {
                Debug.LogWarning($"Hit obstacle: {collision.gameObject.name}");
                const float trueDamage = DAMAGE_BY_OBSTACLE / 25;
                TakeDamageByObstacles(trueDamage);
            }
        }

        [Server]
        private void TakeDamageByObstacles(float damage) {
            var connectionId = connectionToClient.identity;
            if(!connectionId) {
                Debug.LogWarning("Player not found. Failed to take damage");
                return;
            }
            var playerNetId = connectionId.GetComponent<PlayerManager>().netId;
            EventManager.TriggerEvent($"TakeDamage_{playerNetId}", damage);
        }
        
        #endregion
        
        #region GenerateJoints

        public void JointSetup() {
            // Freeze Chassis for accuracy on calculation
            _rigidBody2d.bodyType = RigidbodyType2D.Static;
            _chassisCollider2D = GetComponent<PolygonCollider2D>();
            _chassisBound = _chassisCollider2D.bounds;
            _minX = _chassisBound.min.x;
            _maxX = _chassisBound.max.x;
            _minY = _chassisBound.min.y;
            _maxY = _chassisBound.max.y;
            _heightQuotient = _chassisBound.size.y / HEIGHT_DIVISOR;
            _maxWheelJointY = _minY + (_heightQuotient);
            _jointCount = Random.Range(4, MAX_JOINT_SLOTS + 1);
            CreateColliderMesh();
            GenerateJointPositions();
            // Unfreeze Chassis
            // _rigidBody2d.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // public void HealthInit() {
        //     Health = _jointCount % 2 == 0 ? Random.Range(250, 301) : Random.Range(200, 251);
        // }
        
        private void GenerateJointPositions() {
            GenerateWheelJointPositions(); // Front, rear wheel joint, a must!
            GeneratePositionOfJoint(2, 2); // Weapon Joint, a must! 
            int startSlotId = 3;
            int endSlotId = _jointCount;
            int startTypeId = 3;
            int endTypeId = MAX_JOINT_SLOTS + 1;
            
            int rndTypeId = 0;
            List<int> controllerRange = new List<int>(); // To make sure random int not repeated
            
            for (int slotId = startSlotId; slotId < endSlotId; slotId++) {
                controllerRange.Add(rndTypeId);
                while (controllerRange.Contains(rndTypeId)) {
                    rndTypeId = Random.Range(startTypeId, endTypeId);
                }
                GeneratePositionOfJoint((byte)slotId, (byte)rndTypeId);
            }
        }
        
        private void GenerateWheelJointPositions() {
            var center = _chassisBound.center.x;
            var frontWheelPos = new Vector3() {
                x = Random.Range(center + MIN_WHEEL_DISTANCE, _maxX),
                y = Random.Range(_minY, _maxWheelJointY),
            };
            var frontJoint = new ChassisJoint() {jointId = 0, typeId = 0, type = "Wheel", position = frontWheelPos};
            jointDict.Add(0, frontJoint);
            var deltaY = 0.1f;
            var rearWheelPos = new Vector3() {
                x = center - Mathf.Abs(frontWheelPos.x-center),
                y = Random.Range(frontWheelPos.y - deltaY, frontWheelPos.y + deltaY),
            };
            var rearJoint = new ChassisJoint() {jointId = 1, typeId = 1, type = "Wheel", position = rearWheelPos};
            jointDict.Add(1, rearJoint);
        }
        
        private void GeneratePositionOfJoint(byte id, byte typeId) {
            Vector3 rndPosition = GenerateRandomPosition();
            var type = _jointList[typeId].tag.Split(" ")[0];
            //Debug.LogWarning($"Joint Id {id} - Type id {typeId} - Joint Type: {type}");
            var joint = new ChassisJoint() {jointId = id, typeId = typeId, type = type, position = rndPosition};
            jointDict.Add(id, joint);
        }
        
        private Vector2 GenerateRandomPosition() {
            var rndPosition = _chassisBound.center;
            var rnd = new System.Random();
            var randomizedMeshPoints = _colliderMeshPoints.OrderBy(_ => rnd.Next());
            var deltaY = MESH_SIZE_Y * Mathf.Floor((float)_colliderMeshVerticalCount / (_jointCount - 2));
            //Debug.Log($"DeltaY: {deltaY}");
            foreach (var point in randomizedMeshPoints) {
                var disqualified = false; 
                for (byte id = 2; id < jointDict.Count; id++) {
                    if (jointDict.TryGetValue(id, out ChassisJoint joint)) {
                        var position = joint.position;
                        if (Mathf.Abs(position.y - point.y) >= deltaY) continue; 
                    }
                    else continue;
                    disqualified = true;
                    break;
                }
                if (disqualified) continue;
                rndPosition = point;
                break;
            }
            return rndPosition;
        }
        
        private void CreateColliderMesh() {
            _colliderMeshPoints = new List<Vector2>();
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            CreateMeshBound(out List<Vector2> meshBound);
            foreach (var point in meshBound) {
                if (!_chassisCollider2D.OverlapPoint(point)) continue;
                _colliderMeshPoints.Add(point);
               // CreateMeshPointAt(point);
                var tempY = point.y;
                if (tempY < minY) minY = point.y;
                if (tempY > maxY) maxY = point.y;
            }
            _colliderMeshVerticalCount = (int)(Mathf.Abs(maxY-minY)/MESH_SIZE_Y);
            //Debug.Log($"ColliderMeshPointVerticalCount: {_colliderMeshVerticalCount}");
        }
        
        // Debug method:
        private void CreateMeshPointAt(Vector2 position) {
            var mPoint= Instantiate(_meshPoint, transform, false);
            mPoint.transform.localPosition = position;
        }
        
        private void CreateMeshBound(out List<Vector2> meshPoints) {
            _lowerLimit = _maxWheelJointY + _heightQuotient;
            meshPoints = new List<Vector2>();
            for (var x = _minX; x < _maxX; x += MESH_SIZE_X) {
                for (var y = _lowerLimit; y < _maxY; y += MESH_SIZE_Y) {
                    meshPoints.Add(new Vector2(x, y));
                }
            }
        }
        
        #endregion

        #region RobotComponentInteraction
        
       [Server]
        public void AttachRobotComponentAt(byte jointId, GameObject robotComponent) {
            var nwRobotComponent = robotComponent.GetComponent<NwRobotComponent>();
            var anchoredJoint2d = robotComponent.GetComponent<AnchoredJoint2D>();
            var relativeJoint2d = robotComponent.GetComponent<RelativeJoint2D>();
            if (jointDict.TryGetValue(jointId, out ChassisJoint joint)) {
                if (!_jointComponentDict.Keys.Contains(jointId)) {
                    // Set Chassis Static to attach robotComponent
                    //_rigidBody2d.bodyType = RigidbodyType2D.Static;
                    // robotComponent.transform.SetParent(transform, false);
                    var jointPosition = joint.position;
                    //nwRobotComponent.transform.rotation = transform.rotation;
                    robotComponent.transform.localPosition = jointPosition;
                    nwRobotComponent.parentNetId = netIdentity.netId;
                    nwRobotComponent.localPosition = jointPosition;
                    if (anchoredJoint2d) {
                        anchoredJoint2d.connectedBody = _rigidBody2d;
                        anchoredJoint2d.connectedAnchor = jointPosition;
                    }
                    else {
                        relativeJoint2d.autoConfigureOffset = false;
                        relativeJoint2d.connectedBody = _rigidBody2d;
                        var linearOffset = GetLinearOffsetOf(jointPosition);
                        relativeJoint2d.linearOffset =linearOffset;
                        relativeJoint2d.angularOffset = 0f;
                        // Debug.LogWarning($"jointPosition: {jointPosition} - linearOffset: {linearOffset}");
                    }
                    NetworkServer.Spawn(robotComponent, connectionToClient);
                    var attachedComponentNetId = robotComponent.GetComponent<NetworkIdentity>().netId;
                    _jointComponentDict.Add(jointId, attachedComponentNetId);
                    // Debug.LogWarning($"Attached component id: {attachedComponentNetId}");
                    if (!isFlipped) return;
                    // nwRobotComponent.FlipObject();
                    nwRobotComponent.ReverseDirection();
                    nwRobotComponent.isFlipped = true;
                }
                else {
                    DetachRobotComponentAt(jointId);
                    AttachRobotComponentAt(jointId, robotComponent);
                }
            }
            else {
                Debug.LogError($"Invalid jointId: {jointId}");
            }
        }
        
        [Server]
        public void DetachRobotComponentAt(byte jointId) {
            var objectId = _jointComponentDict[jointId];
            var robotComponent = GetSpawnedObject(objectId);
            if (robotComponent is not null) {
                RobotComponentDisassemble(robotComponent);
            }
            else {
                Debug.LogWarning("Trying to detach null component.");
            }
            _jointComponentDict.Remove(jointId);
        }

        [Server]
        private void RobotComponentDisassemble(GameObject robotComponent) {
            robotComponent.transform.parent = null;
            var anchoredJoint2d = robotComponent.GetComponent<AnchoredJoint2D>();
            var relativeJoint2d = robotComponent.GetComponent<RelativeJoint2D>();
            if (anchoredJoint2d) {
                anchoredJoint2d.connectedBody = null;
                anchoredJoint2d.connectedAnchor = new Vector2();
            }
            else {
                relativeJoint2d.connectedBody = null;
            }
            
            Destroy(robotComponent);
        }
        
        #endregion
        
        #endregion
        
        #region Client
        
        [Client]
        protected override void OnSpawnClient() {
            var battleView = GameObject.FindGameObjectWithTag("BattleView");
            transform.SetParent(battleView.transform,false);
            if (isClientOnly) {
                Remove<ChassisStateManager>(gameObject);
                GetComponent<SpringJoint2D>().enabled = false;
                GetComponent<Rigidbody2D>().isKinematic = true;
                //_grapplingRope.isGrappling = true; // Avoid chassis on clientOnly to enable SpringJoint2D
                SetJointsActive();
            }
            if (hasAuthority) {
                CmdInitChassisStateManager();
                //CmdStartGrappling();
                DisplayJointCards();
            }
            // Debug.Log($"Instance Id on Client: {GetInstanceID()}");
            // CmdSetRobotComponentDynamic();
        }
        
        [Client]
        private void OnGrappledChange(bool oldValue, bool newValue) {
            if (newValue == false) {
                _grapplingLauncher.RemoveGrappling();
            }
            else {
                _grapplePoint = hasAuthority ? _grapplePointLeft : _grapplePointRight;
                _grapplingLauncher.StartGrappling(_grapplePoint);
            }
            
        }
        
        [Client]
        private void DisplayJointCards() {
            foreach (var joint in jointDict.Values) {
                var jointType = joint.type;
                var jointCardUI = GameObject.FindGameObjectWithTag("JointCardUI");
                var jointCardPrefab = Resources.Load(JOINT_CARD_PREFAB_PATH) as GameObject;
                var jointCardTemplate = Instantiate(jointCardPrefab, jointCardUI.transform);
                var jointCardController = jointCardTemplate.GetComponent<NwJointCard>();
                jointCardController.SetupJointCard(joint, this);
            }
        }
        
        #endregion

        #region Shared

        private Vector3 GetPositionOf(Vector3 position) {
            if(isFlipped) position.x *= -1;
            return position;
        }

        private Vector2 GetLinearOffsetOf(Vector2 vector2) {
            if (isFlipped) vector2.y *= -1;
            else vector2 *= -1;
            return vector2;
        }
        
        private string CreateEventName(string objectName) {
            return objectName + _instanceID;
        }
        private void GetJointList() {
            for (int i = 1; i <= MAX_JOINT_SLOTS + 1; i++) {
                var joint = transform.Find(JOINT_NAME + i).gameObject;
                if (joint == null) Debug.LogError("Joint not found");
                _jointList.Add(joint); 
            }  
        }
        
        private void SetJointsActive() {
            for (int id = 0; id < jointDict.Count; id++) {
                if (!jointDict.TryGetValue((byte)id, out ChassisJoint joint)) continue;
                var jointListId = joint.typeId;
                _jointList[jointListId].SetActive(true);
                _jointList[jointListId].transform.localPosition = joint.position;
            }
        }
        
        public void HideJoints() {
            foreach (var joint in _jointList) {
                joint.SetActive(false);
            }
        }

        public int GetJointId(GameObject jointObject) {
            var jointId = _jointList.FindIndex(j => j == jointObject);
            if (jointId == -1) Debug.Log("Joint not found");
            return jointId;
        }

        private GameObject GetSpawnedObject(uint id) {
            return NetworkSupporter.GetSpawnedObject(id);
        }
        
        #endregion

        // private void OnDestroy() {
        //     Debug.Log("Chassis Destroyed!");
        // }
        
        private void OnApplicationQuit() {
            _quit = true;
        }
    }
    
}
