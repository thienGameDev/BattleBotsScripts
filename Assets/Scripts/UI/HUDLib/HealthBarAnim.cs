using UnityEngine;
using UnityEngine.UI;

namespace UI.HUDLib {
    public class HealthBarAnim : MonoBehaviour {
        private Image _healthBarRed;
        private float _vel;
        private float _smoothTime = .25f;
        private float _targetFillAmount;

        public float TargetFillAmount {
            set => _targetFillAmount = value;
        }

        private void Awake() {
            _healthBarRed = GetComponent<Image>();
            _targetFillAmount = 1;
        }

        private void Update() {
            _healthBarRed.fillAmount = Mathf.SmoothDamp(_healthBarRed.fillAmount, _targetFillAmount, ref _vel, _smoothTime);
        }
    }
}