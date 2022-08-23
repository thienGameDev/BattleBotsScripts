using ComponentStates;
using Mirror;
using UnityEngine;

namespace RobotComponentLib.RangedWeaponLib {
    public class NwRangedWeapon : NwWeapon {
        [SerializeField] private GameObject rangedChildPrefab;
        [SerializeField] private int maxCount;
        private Pool<GameObject> _rangedChildren;
        private int _currentCount;
        private const float RELOAD_TIME = .5f;
        private const float ON_PLACEMENT_TIME = 1.5f;
        private Coroutine _fireCoroutine;
        private string _enterStaticStateEvent;
        private string _enterDynamicStateEvent;
        private GameObject _currentChild;
        private bool _spawned;
        private bool _isAnyChild;
        private bool _isFiring;
        private float _placementCountDown;
        private float _reloadCountDown;
        
        #region Server
        
        [ServerCallback]
        private void Update() {
            if (_spawned || !NetworkClient.spawned.TryGetValue(netId, out var networkIdentity)) return;
            InitializeRangedChild();
            _spawned = true;
        }

        [Server]
        public void StartFiring() {
            // Debug.Log($"RangedParent {netId} start firing!");
            _placementCountDown = ON_PLACEMENT_TIME;
            _reloadCountDown = RELOAD_TIME;
            _isFiring = true;
        }
    
        [ServerCallback]
        private void FixedUpdate() {
            if(!_spawned) return;
            if(!_isFiring) return;
            Fire();
        }

        [Server]
        public void StopFiring() {
            _isFiring = false;
            if(_isAnyChild) SpawnRangedWeaponChild();
        }
        
        [Server]
        private void InitializeRangedChild() {
            _rangedChildren = new Pool<GameObject>(CreateNewChild, maxCount);
            SpawnRangedWeaponChild();
        }
        
        [Server]
        private GameObject CreateNewChild() {
            GameObject nextChild = Instantiate(rangedChildPrefab, transform, false);
            if (isFlipped) {
                nextChild.GetComponent<NwRobotComponent>().isFlipped = true;
            }
            nextChild.GetComponent<RangedWeaponChildStateManager>().rangedParent = gameObject;
            nextChild.GetComponent<NwRobotComponent>().parentNetId = netId;
            nextChild.GetComponent<NwRobotComponent>().Damage = Damage;
            nextChild.name = $"_{rangedChildPrefab.name}_pooled_{netId}_{_currentCount}";
            nextChild.SetActive(false);
            _currentCount++;
            return nextChild;
        }
        
        [Server]
        private GameObject GetChild()
        {
            GameObject nextChild = _rangedChildren.Get();
            // set position/rotation and set active
            nextChild.transform.parent = transform;
            nextChild.transform.position = transform.position;
            nextChild.transform.rotation = transform.rotation;
            nextChild.SetActive(true);
            return nextChild;
        }
        
        [Server]
        public void ReturnChild(GameObject spawned)
        {
            // disable object
            spawned.SetActive(false);

            // add back to pool
            _rangedChildren.Return(spawned);
        }

        [Server]
        private void Fire() {
            if (_placementCountDown < 0) {
                if(_isAnyChild) LaunchChild();
                if (_reloadCountDown < 0) {
                    SpawnRangedWeaponChild();
                    StartFiring();
                }
                else {
                    _reloadCountDown -= Time.deltaTime;
                }
            }
            else {
                _placementCountDown -= Time.deltaTime;
            }
        }

        [Server]
        private void LaunchChild() {
            //Debug.LogWarning($"RangedParent {netId} called LaunchChild(): {_currentChild}!");
            _currentChild.GetComponent<RangedWeaponChildStateManager>().Execute();
            _isAnyChild = false;
        }

        [Server]
        private void SpawnRangedWeaponChild() {
            _currentChild = GetChild();
            NetworkServer.Spawn(_currentChild, connectionToClient);
            _isAnyChild = true;
            // Debug.LogWarning($"{_currentChild} is spawned with {connectionToClient}");
        }
        
        #endregion


        #region Shared
        
        #endregion

        #region Client
        
        #endregion
    }
}