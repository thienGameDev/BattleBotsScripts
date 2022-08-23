using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI {
    public class NwChassisUI : MonoBehaviour {
        private float _health;
        private List<GameObject> _jointList;
        private List<Vector3> _jointDict;
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
        
        private void GetJointList() {
            for (int i = 1; i <= MAX_JOINT_SLOTS + 1; i++) {
                var joint = transform.Find(JOINT_NAME + i).gameObject;
                if (joint == null) Debug.LogError("Joint not found");
                _jointList.Add(joint); 
            }  
        }

        private void Awake() {
            _jointDict = new List<Vector3>();
            _jointList = new List<GameObject>();
            GetJointList();
            SetupJoints();
            SetJointsActive();
        }

        private void Start() {
            SetSortingLayer();
        }

        private void SetSortingLayer() {
            var spriteRenders = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in spriteRenders) {
                var sortingOrder = sr.sortingOrder;
                sr.sortingLayerName = "UI";
                sr.sortingOrder = sortingOrder;
            }
        }

        #region Public Method
        
        public List<Vector3> GetJointDict() {
            return _jointDict ?? null;
        }

        public float GetChassisHealth() {
            if (_health != 0) return _health;
            HealthInit();
            return _health;
        }
        
        #endregion
        
        
        #region GenerateJoints

        private void SetupJoints() {
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
        }
        
        private void HealthInit() {
            if (_jointCount != 0) _health = _jointCount % 2 == 0 ? Random.Range(250, 301) : Random.Range(200, 251);
            else Debug.LogWarning("No joints. Please setup joints first!");
        }
        
        private void SetJointsActive() {
            for (var id = 0; id < _jointDict.Count; id++) {
                var jointListId = (int)_jointDict[id].z;
                _jointList[jointListId].SetActive(true);
                var jointPosition = new Vector3() {
                    x = _jointDict[id].x,
                    y = _jointDict[id].y
                };
                _jointList[jointListId].transform.localPosition = jointPosition;
            }
        }
        
        private void GenerateJointPositions() {
            GenerateWheelJointPositions(); // Front, rear wheel joint, a must!
            GeneratePositionOfJoint(2); // Weapon Joint, a must! 
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
                GeneratePositionOfJoint((byte)rndTypeId);
            }
        }
        
        private void GenerateWheelJointPositions() {
            var center = _chassisBound.center.x;
            var frontWheelPos = new Vector3() {
                x = Random.Range(center + MIN_WHEEL_DISTANCE, _maxX),
                y = Random.Range(_minY, _maxWheelJointY),
                z = 0
            };
            //var frontJoint = new NwChassis.ChassisJoint() {jointId = 0, typeId = 0, type = "Wheel", position = frontWheelPos};
            _jointDict.Add(frontWheelPos);
            var deltaY = 0.1f;
            var rearWheelPos = new Vector3() {
                x = center - Mathf.Abs(frontWheelPos.x-center),
                y = Random.Range(frontWheelPos.y - deltaY, frontWheelPos.y + deltaY),
                z = 1
            };
            // var rearJoint = new NwChassis.ChassisJoint() {jointId = 1, typeId = 1, type = "Wheel", position = rearWheelPos};
            _jointDict.Add(rearWheelPos);
        }
        
        private void GeneratePositionOfJoint(byte typeId) {
            Vector3 rndPosition = GenerateRandomPosition();
            rndPosition.z = typeId;
            // var type = _jointList[typeId].tag.Split(" ")[0];
            //Debug.LogWarning($"Joint Id {id} - Type id {typeId} - Joint Type: {type}");
            // var joint = new NwChassis.ChassisJoint() {jointId = id, typeId = typeId, type = type, position = rndPosition};
            _jointDict.Add(rndPosition);
        }
        
        private Vector2 GenerateRandomPosition() {
            var rndPosition = _chassisBound.center;
            var rnd = new System.Random();
            var randomizedMeshPoints = _colliderMeshPoints.OrderBy(_ => rnd.Next());
            var deltaY = MESH_SIZE_Y * Mathf.Floor((float)_colliderMeshVerticalCount / (_jointCount - 2));
            //Debug.Log($"DeltaY: {deltaY}");
            foreach (var point in randomizedMeshPoints) {
                var disqualified = false; 
                for (byte id = 2; id < _jointDict.Count; id++) {
                    var position = _jointDict[id];
                    if (Mathf.Abs(position.y - point.y) >= deltaY) continue;
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
    }
}