using UnityEngine;

namespace EmergencyXR.Locomotion
{
    /// <summary>
    /// 양손의 전후 흔들림 강도를 측정해 이동 속도 배율을 제공합니다.
    /// </summary>
    public class ArmSwingSpeedBoost : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _leftHand;

        [SerializeField]
        private Transform _rightHand;

        [SerializeField]
        private Transform _forwardReference;

        [Header("Boost")]
        [SerializeField]
        private float _activationSpeed = 0.35f;

        [SerializeField]
        private float _fullBoostSpeed = 1.4f;

        [SerializeField]
        private float _maxSpeedMultiplier = 1.9f;

        [Header("Filter")]
        [SerializeField, Range(0f, 1f)]
        private float _handPositionSmoothing = 0.25f;

        [SerializeField]
        private float _boostRisePerSecond = 2.8f;

        [SerializeField]
        private float _boostFallPerSecond = 2.0f;

        [SerializeField]
        private float _dropHoldTime = 0.1f;

        public float CurrentBoost01 => _currentBoost01;
        public float CurrentSpeedMultiplier => Mathf.Lerp(1f, _maxSpeedMultiplier, _currentBoost01);

        private Vector3 _smoothedLeftLocalPos;
        private Vector3 _smoothedRightLocalPos;
        private Vector3 _lastLeftLocalPos;
        private Vector3 _lastRightLocalPos;
        private bool _initialized;
        private float _currentBoost01;
        private float _belowThresholdTimer;

        private void OnEnable()
        {
            InitializeState();
        }

        private void Update()
        {
            if (_leftHand == null || _rightHand == null || _forwardReference == null)
            {
                _currentBoost01 = 0f;
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            if (!_initialized)
            {
                InitializeState();
            }

            // 컨트롤러 미세 떨림 노이즈를 줄이기 위해 위치를 먼저 평활합니다.
            float lerpT = 1f - Mathf.Pow(1f - Mathf.Clamp01(_handPositionSmoothing), dt * 60f);
            Vector3 leftLocalPos = _forwardReference.InverseTransformPoint(_leftHand.position);
            Vector3 rightLocalPos = _forwardReference.InverseTransformPoint(_rightHand.position);
            _smoothedLeftLocalPos = Vector3.Lerp(_smoothedLeftLocalPos, leftLocalPos, lerpT);
            _smoothedRightLocalPos = Vector3.Lerp(_smoothedRightLocalPos, rightLocalPos, lerpT);

            Vector3 leftVelocity = (_smoothedLeftLocalPos - _lastLeftLocalPos) / dt;
            Vector3 rightVelocity = (_smoothedRightLocalPos - _lastRightLocalPos) / dt;

            _lastLeftLocalPos = _smoothedLeftLocalPos;
            _lastRightLocalPos = _smoothedRightLocalPos;

            // 리그 이동/회전 영향을 제거한 로컬 속도에서 지면 수평(XZ) 성분만 사용합니다.
            leftVelocity.y = 0f;
            rightVelocity.y = 0f;
            float leftHorizontalSpeed = leftVelocity.magnitude;
            float rightHorizontalSpeed = rightVelocity.magnitude;
            float effectiveSwingSpeed = leftHorizontalSpeed + rightHorizontalSpeed;

            float targetBoost = Mathf.InverseLerp(_activationSpeed, _fullBoostSpeed, effectiveSwingSpeed);

            if (targetBoost <= 0f)
            {
                _belowThresholdTimer += dt;
                if (_belowThresholdTimer < _dropHoldTime)
                {
                    targetBoost = _currentBoost01;
                }
            }
            else
            {
                _belowThresholdTimer = 0f;
            }

            float rate = targetBoost > _currentBoost01 ? _boostRisePerSecond : _boostFallPerSecond;
            _currentBoost01 = Mathf.MoveTowards(_currentBoost01, targetBoost, Mathf.Max(0.01f, rate) * dt);
        }

        private void InitializeState()
        {
            _initialized = true;
            _belowThresholdTimer = 0f;

            if (_leftHand != null)
            {
                Vector3 leftLocalPos = _forwardReference.InverseTransformPoint(_leftHand.position);
                _smoothedLeftLocalPos = leftLocalPos;
                _lastLeftLocalPos = leftLocalPos;
            }

            if (_rightHand != null)
            {
                Vector3 rightLocalPos = _forwardReference.InverseTransformPoint(_rightHand.position);
                _smoothedRightLocalPos = rightLocalPos;
                _lastRightLocalPos = rightLocalPos;
            }
        }
    }
}
