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
        private float _activationSpeed = 1.0f;

        [SerializeField]
        private float _fullBoostSpeed = 3.0f;

        [SerializeField]
        private float _maxSpeedMultiplier = 1.9f;

        [Header("Filter")]
        [SerializeField, Range(0f, 1f)]
        private float _smoothing = 0.18f;

        [SerializeField]
        private float _dropHoldTime = 0.12f;

        public float CurrentBoost01 => _currentBoost01;
        public float CurrentSpeedMultiplier => Mathf.Lerp(1f, _maxSpeedMultiplier, _currentBoost01);

        private Vector3 _lastLeftPos;
        private Vector3 _lastRightPos;
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

            Vector3 leftVelocity = (_leftHand.position - _lastLeftPos) / dt;
            Vector3 rightVelocity = (_rightHand.position - _lastRightPos) / dt;

            _lastLeftPos = _leftHand.position;
            _lastRightPos = _rightHand.position;

            Vector3 forward = _forwardReference.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            float leftForwardSpeed = Mathf.Abs(Vector3.Dot(leftVelocity, forward));
            float rightForwardSpeed = Mathf.Abs(Vector3.Dot(rightVelocity, forward));
            float swingSpeed = leftForwardSpeed + rightForwardSpeed;

            float targetBoost = Mathf.InverseLerp(_activationSpeed, _fullBoostSpeed, swingSpeed);

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

            _currentBoost01 = Mathf.Lerp(_currentBoost01, targetBoost, 1f - Mathf.Pow(1f - _smoothing, dt * 60f));
        }

        private void InitializeState()
        {
            _initialized = true;
            _belowThresholdTimer = 0f;

            if (_leftHand != null)
            {
                _lastLeftPos = _leftHand.position;
            }

            if (_rightHand != null)
            {
                _lastRightPos = _rightHand.position;
            }
        }
    }
}
