using System.Collections.Generic;
using UnityEngine;

namespace _Camera {
    public class OrthographicZoom : MonoBehaviour
    {
        [SerializeField] private float minOrthoSize = 5f;
        [SerializeField] private float maxOrthoSize = 3.5f;
        [SerializeField] private float smoothTime = .2f;
        [SerializeField] private float offsetY = .9f;

        private const string CHASSIS_TAG = "Chassis";
        private GameObject[] _chassis;
        private List<Transform> _targets = new List<Transform>();
        private Camera _cam;
        private Vector2 _velocity;
        private float _aspect;
        private float _worldHeight;
        private float _worldWidth;
        private float _maxY = 1.18f;
        private float _minY = -0.5f;
        private void Start(){
            _chassis = GameObject.FindGameObjectsWithTag(CHASSIS_TAG);
            foreach (GameObject chassis in _chassis){
                _targets.Add(chassis.transform);
            }      
            _cam = Camera.main;
            _aspect = (float) Screen.width / Screen.height;
        }

        private void LateUpdate(){
            if (_targets.Count == 0) return;
            Zoom();
            Move();
        }
        

        private void Move(){
            Vector2 centerPoint = GetCenterPoint();
            if(centerPoint == Vector2.one) return;
            SetCamLimit(ref centerPoint);
            transform.position = Vector2.SmoothDamp(transform.position, centerPoint, ref _velocity, smoothTime);
        }

        private void SetCamLimit(ref Vector2 camPos) {
            if (camPos.y > _maxY) camPos.y = _maxY;
            if (camPos.y < _minY) camPos.y = _minY;
        }
        private void Zoom(){
            _worldHeight = _cam.orthographicSize * 2;
            _worldWidth = _worldHeight * _aspect;
            var distance = GetDistance();
            float newOrthographicSize = Mathf.Lerp(minOrthoSize,maxOrthoSize, 
                distance/_worldWidth);
            //Debug.Log($"Distance/Screen width: {GetDistance()} / {_worldWidth}");
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, newOrthographicSize, Time.deltaTime);
        }
        private float GetDistance() {
            
            Bounds bounds;
            
            if(_targets.Count > 0 && _targets[0]) 
                bounds = new Bounds(_targets[0].position, Vector3.zero);
            else if (_targets.Count > 1 && _targets[1])
                bounds = new Bounds(_targets[1].position, Vector3.zero);
            else return _worldWidth;
            
            foreach (Transform target in _targets) {
                if (!target) {
                    _targets.Remove(target);
                     return _worldWidth;
                }
                bounds.Encapsulate(target.position);
            }
            return bounds.size.x > bounds.size.y ? bounds.size.x : bounds.size.y * _aspect;
        }
        private Vector2 GetCenterPoint(){
            
            Bounds bounds;
            
            if(_targets.Count > 0 && _targets[0]) 
                bounds = new Bounds(_targets[0].position, Vector3.zero);
            else if (_targets.Count > 1 && _targets[1])
                bounds = new Bounds(_targets[1].position, Vector3.zero);
            else return Vector2.zero;
           
            foreach (Transform target in _targets) {
                if (!target) {
                    _targets.Remove(target);
                    return new Vector2(_targets[0].position.x, _targets[0].position.y + offsetY);
                }
                bounds.Encapsulate(target.position);
            }
            Vector2 centerOffset = new Vector2(bounds.center.x, bounds.center.y + offsetY);
            return centerOffset;
        }
    }
}
