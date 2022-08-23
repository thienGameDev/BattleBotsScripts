using System;
using System.Collections;
using UnityEngine;

namespace ObstacleLib {
    public class MovableObstacle : MonoBehaviour {
        [SerializeField] private GameObject staticBody;
        [SerializeField] private bool repeated;
        [SerializeField] private bool reverseRotation;
        [SerializeField] private Transform target;
        [SerializeField] private float pauseEnd = .5f;
        [SerializeField] private float movingSpeed = .5f;
        
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _maxDistance;
        private Coroutine _resetPositions;
        private bool _isReverseTranslation;
        private Vector3 _direction;

        private void OnEnable() {
            // Debug.LogWarning($"{gameObject.name} enable");
            _resetPositions = null;
            _isReverseTranslation = false;
            if (reverseRotation) ReverseRotation();
            _startPosition = staticBody.transform.localPosition;
            _targetPosition = target.localPosition;
            _maxDistance = Vector3.Distance(_targetPosition, _startPosition);
            _direction = (_targetPosition - _startPosition).normalized;
        }
        
        private void ReverseRotation() {
            var wheelJoint2D = GetComponentInChildren<WheelJoint2D>();
            if (!wheelJoint2D) return;
            var motor = wheelJoint2D.motor;
            motor.motorSpeed *= -1;
            wheelJoint2D.motor = motor;
        }
        
        // Update is called once per frame
        private void FixedUpdate() {
            var distance = Vector3.Distance(staticBody.transform.localPosition, _startPosition);
            if (distance > _maxDistance) {
                if(!repeated) {
                    if(_isReverseTranslation) {
                        _isReverseTranslation = false;
                        enabled = false;
                    }
                    return;
                }
                _resetPositions ??= StartCoroutine(ResetPositions());
                return;
            }
            // Debug.Log($"current distance: {distance}");
            staticBody.transform.Translate(_direction * (Time.deltaTime * movingSpeed));
        }
        
        private IEnumerator ResetPositions() {
            yield return new WaitForSeconds(pauseEnd);
            ReverseTranslation();
            _resetPositions = null;
        }

        public void ReverseTranslation() {
            (_startPosition, _targetPosition) = (_targetPosition, _startPosition);
            _direction *= -1;
            _isReverseTranslation = true;
        }

        private void OnDisable() {
            // Debug.LogWarning($"{gameObject.name} disable");
            staticBody.transform.localPosition = new Vector3();
        }
    }
}
