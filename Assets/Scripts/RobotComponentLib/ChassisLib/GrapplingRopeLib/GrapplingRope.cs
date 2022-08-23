using System;
using UnityEngine;

namespace RobotComponentLib.ChassisLib.GrapplingRopeLib {
    public class GrapplingRope : MonoBehaviour
    {
        [Header("General Settings:")]
        [SerializeField] private int precision = 40;
        [Range(0, 20)] [SerializeField] private float straightenLineSpeed = 5;

        [Header("Rope Animation Settings:")]
        public AnimationCurve ropeAnimationCurve;
        [Range(0.01f, 4)] [SerializeField] private float startWaveSize = 2;
        private float _waveSize = 0;

        [Header("Rope Progression:")]
        public AnimationCurve ropeProgressionCurve;
        [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;

        private float _moveTime = 0;

        [HideInInspector] public bool isGrappling = true;

        private bool _straightLine = true;
        private GrapplingLauncher _grapplingLauncher;
        private LineRenderer _lineRenderer;
        
        private void Awake() {
            _grapplingLauncher = GetComponentInParent<GrapplingLauncher>();
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void OnEnable()
        {
            _moveTime = 0;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = precision;
            _waveSize = startWaveSize;
            _straightLine = false;

            LinePointsToFirePoint();

            _lineRenderer.enabled = true;
        }

        private void OnDisable()
        {
            _lineRenderer.enabled = false;
            isGrappling = false;
        }

        private void LinePointsToFirePoint()
        {
            for (int i = 0; i < precision; i++)
            {
                _lineRenderer.SetPosition(i, _grapplingLauncher.firePoint.position);
            }
        }

        private void Update()
        {
            _moveTime += Time.deltaTime;
            DrawRope();
        }

        void DrawRope()
        {
            if (!_straightLine)
            {
                if (Math.Abs(_lineRenderer.GetPosition(precision - 1).x - _grapplingLauncher.grapplePoint.x) == 0)
                {
                    _straightLine = true;
                }
                else
                {
                    DrawRopeWaves();
                }
            }
            else
            {
                if (!isGrappling)
                {
                    _grapplingLauncher.Grapple();
                    isGrappling = true;
                }
                if (_waveSize > 0)
                {
                    _waveSize -= Time.deltaTime * straightenLineSpeed;
                    DrawRopeWaves();
                }
                else
                {
                    _waveSize = 0;

                    if (_lineRenderer.positionCount != 2) { _lineRenderer.positionCount = 2; }

                    DrawRopeNoWaves();
                }
            }
        }

        void DrawRopeWaves()
        {
            for (int i = 0; i < precision; i++)
            {
                float delta = (float)i / ((float)precision - 1f);
                Vector2 offset = Vector2.Perpendicular(_grapplingLauncher.grappleDistanceVector).normalized * (ropeAnimationCurve.Evaluate(delta) * _waveSize);
                Vector2 targetPosition = Vector2.Lerp(_grapplingLauncher.firePoint.position, _grapplingLauncher.grapplePoint, delta) + offset;
                Vector2 currentPosition = Vector2.Lerp(_grapplingLauncher.firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(_moveTime) * ropeProgressionSpeed);

                _lineRenderer.SetPosition(i, currentPosition);
            }
        }

        void DrawRopeNoWaves()
        {
            _lineRenderer.SetPosition(0, _grapplingLauncher.firePoint.position);
            _lineRenderer.SetPosition(1, _grapplingLauncher.grapplePoint);
        }
    }
}
