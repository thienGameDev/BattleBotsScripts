using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

namespace UI.Controllers {
    public class ChassisInventoryController: StaticInstance<ChassisInventoryController> {
        [SerializeField] private Button leftBtn;
        [SerializeField] private Button rightBtn;
        [SerializeField] private Button okayBtn;
        [SerializeField] private TMP_Text chassisName;
        [SerializeField] private TMP_Text healthPoint;
        
        private string _chassisPrefabPath = PathManager.ChassisPath;
        private int _currentChassisId;
        private GameObject _currentChassis;
        private Dictionary<GameObject, PlayerData.Chassis> _chassisList;
        private PlayerData.Chassis _selectedChassis;

        protected override void Awake() {
            base.Awake();
            _chassisList = new Dictionary<GameObject, PlayerData.Chassis>();
            var chassisPrefabs = Resources.LoadAll(_chassisPrefabPath);
            foreach (var chassisPrefab in chassisPrefabs) {
                var prefab = chassisPrefab as GameObject;
                var chassis = Instantiate(prefab, transform, false);
                chassis.SetActive(false);
                chassis.name = chassisPrefab.name;
                RemoveAllAttachComponents(chassis);
                RemoveGrapplingFeature(chassis);
                AttachChassisUIComponent(chassis);
                _chassisList.Add(chassis, new PlayerData.Chassis() {
                    name = chassis.name.Split(".")[^1], 
                    prefab = prefab
                });
            }
        }

        private void Start() {
            SetCurrentChassis();
            leftBtn.onClick.AddListener(LeftBtnHandler);
            rightBtn.onClick.AddListener(RightBtnHandler);
            okayBtn.onClick.AddListener(OkayBtnHandler);
        }

        private void LeftBtnHandler() {
            _currentChassis.SetActive(false);
            _currentChassisId--;
            if (_currentChassisId < 0) 
                _currentChassisId = _chassisList.Count - 1;
            SetCurrentChassis();
        }

        private void RightBtnHandler() {
            _currentChassis.SetActive(false);
            _currentChassisId++;
            if (_currentChassisId >= _chassisList.Count)
                _currentChassisId = 0;
            SetCurrentChassis();
        }

        private void OkayBtnHandler() {
            PlayerData.Instance.localPlayer = new PlayerData.Player() {
                name = $"Player #{Random.Range(0, 99999)}",
                chassis = _chassisList[_currentChassis]
            };
            SceneManager.LoadScene("Lobby");
        }

        private void SetCurrentChassis() {
            _currentChassis = _chassisList.Keys.ElementAt(_currentChassisId);
            _currentChassis.SetActive(true);
            var chassisData = _chassisList[_currentChassis];
            chassisData.health = GetChassisHp();
            chassisName.text = chassisData.name;
            healthPoint.text = $"{chassisData.health}HP";
            chassisData.jointPositions = GetJointPositions();
            _chassisList[_currentChassis] = chassisData;
        }

        private float GetChassisHp() {
            var nwChassisUI = _currentChassis.GetComponent<NwChassisUI>();
            return nwChassisUI.GetChassisHealth();
        }
        
        private List<Vector3> GetJointPositions() {
            var nwChassisUI = _currentChassis.GetComponent<NwChassisUI>();
            return nwChassisUI.GetJointDict();
        }

        private void RemoveAllAttachComponents(GameObject chassis) {
            var attachComponents = chassis.GetComponents<Component>();
            foreach (var component in attachComponents.Reverse()) {
                if(component is Transform or PolygonCollider2D) continue;
                DestroyImmediate(component);
            }
        }
        
        private void RemoveGrapplingFeature(GameObject chassis) {
            var grapplingLauncher = chassis.transform.Find("GrapplingLauncher").gameObject;
            DestroyImmediate(grapplingLauncher);
        }

        private void AttachChassisUIComponent(GameObject chassisUI) {
            chassisUI.AddComponent<NwChassisUI>();
        }
    }
}