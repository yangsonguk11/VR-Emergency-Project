using EmergencyXR.Interaction;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EmergencyXR.FireExtinguisher
{
    /// <summary>
    /// 안전핀을 지정 축으로만 당길 수 있게 제한하고, 누적 거리에 도달하면 분리 처리합니다.
    /// </summary>
    [RequireComponent(typeof(GrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class SafetyPinPullConstraint : MonoBehaviour
    {
        public enum PullAxis
        {
            LocalUp,
            LocalRight,
            LocalForward
        }

        [Header("Required References")]
        [SerializeField]
        private GrabInteractable _pinInteractable;

        [SerializeField]
        private IndexOnlyGrabbable _indexOnlyGate;

        [SerializeField]
        private Rigidbody _pinRigidbody;

        [SerializeField]
        private Transform _pullReference;

        [Header("Pull Constraint")]
        [SerializeField]
        private PullAxis _pullAxis = PullAxis.LocalUp;

        [SerializeField]
        private bool _pullTowardNegativeAxis = false;

        [SerializeField]
        private bool _allowEitherDirection = false;

        [SerializeField]
        private float _pullDistance = 0.06f;

        [SerializeField]
        private bool _returnToSocketOnRelease = true;

        [Header("Events")]
        [SerializeField]
        private UnityEvent _whenPinRemoved;

        public bool IsPinRemoved { get; private set; }
        public float CurrentPullDistance { get; private set; }

        private Vector3 _startLocalPosition;
        private Quaternion _startLocalRotation;
        private Transform _originalParent;
        private bool _hasDetachedFromBody;
        private bool _gravityEnabledAfterRelease;
        private float _currentSignedDistance;
        private float _releaseStabilizeUntilTime;

        private void Reset()
        {
            _pinInteractable = GetComponent<GrabInteractable>();
            _indexOnlyGate = GetComponent<IndexOnlyGrabbable>();
            _pinRigidbody = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            // 외부 오배정을 막기 위해 같은 오브젝트의 GrabInteractable만 사용합니다.
            _pinInteractable = GetComponent<GrabInteractable>();

            if (_pinRigidbody == null)
            {
                _pinRigidbody = GetComponent<Rigidbody>();
            }

            // 혹시 컴포넌트가 유실된 경우 런타임에서라도 핀 전용 RB를 보장합니다.
            if (_pinRigidbody == null)
            {
                _pinRigidbody = gameObject.AddComponent<Rigidbody>();
            }

            // 핀을 잡았을 때 본체가 같이 끌려가지 않도록 핀 전용 Rigidbody를 강제합니다.
            if (_pinInteractable != null && _pinInteractable.Rigidbody != _pinRigidbody)
            {
                _pinInteractable.InjectRigidbody(_pinRigidbody);
            }

            if (_pinRigidbody != null)
            {
                _pinRigidbody.isKinematic = true;
                _pinRigidbody.useGravity = false;
            }

            if (_pullReference == null)
            {
                _pullReference = transform.parent;
            }

            if (_pullReference == null)
            {
                _pullReference = transform;
            }

            // 분리 전 기준 오프셋을 pullReference 공간으로 저장합니다.
            _startLocalPosition = _pullReference.InverseTransformPoint(transform.position);
            _startLocalRotation = Quaternion.Inverse(_pullReference.rotation) * transform.rotation;
            _originalParent = transform.parent;
            _hasDetachedFromBody = false;
            _gravityEnabledAfterRelease = false;
            _currentSignedDistance = 0f;
            _releaseStabilizeUntilTime = 0f;
        }

        private void Update()
        {
            if (IsPinRemoved)
            {
                return;
            }

            if (Time.unscaledTime < _releaseStabilizeUntilTime)
            {
                ForceAxisOnlyPose(_returnToSocketOnRelease ? 0f : _currentSignedDistance);
            }
        }

        private void LateUpdate()
        {
            if (IsPinRemoved)
            {
                // 분리 후 중력 적용은 인덱스 게이트가 아니라 실제 선택 해제 기준으로 처리합니다.
                if (!HasSelectingInteractor())
                {
                    EnsureFreePhysics();
                    if (!_gravityEnabledAfterRelease)
                    {
                        _gravityEnabledAfterRelease = true;
                    }
                }
                return;
            }

            if (!IsGrabInProgress())
            {
                _releaseStabilizeUntilTime = Time.unscaledTime + 0.05f;
                if (_returnToSocketOnRelease)
                {
                    CurrentPullDistance = 0f;
                    _currentSignedDistance = 0f;
                    ForceAxisOnlyPose(0f);
                }
                else
                {
                    // 마지막 부호 방향 위치를 유지해 릴리즈 시 반대축으로 튀는 문제를 막습니다.
                    ForceAxisOnlyPose(_currentSignedDistance);
                }
                return;
            }

            Vector3 axis = GetAxisWorld();
            Vector3 startWorld = GetStartWorldPosition();
            Vector3 deltaFromStart = transform.position - startWorld;
            float signedAxisDistance = Vector3.Dot(deltaFromStart, axis);

            float progressDistance;
            float constrainedSignedDistance;

            if (_allowEitherDirection)
            {
                progressDistance = Mathf.Abs(signedAxisDistance);
                constrainedSignedDistance = Mathf.Clamp(signedAxisDistance, -_pullDistance, _pullDistance);
            }
            else
            {
                progressDistance = _pullTowardNegativeAxis ? -signedAxisDistance : signedAxisDistance;
                if (progressDistance < 0f)
                {
                    progressDistance = 0f;
                }

                float constrainedProgress = Mathf.Min(progressDistance, _pullDistance);
                constrainedSignedDistance = _pullTowardNegativeAxis ? -constrainedProgress : constrainedProgress;
            }

            ForceAxisOnlyPose(constrainedSignedDistance);
            _currentSignedDistance = constrainedSignedDistance;
            float constrainedDistance = Mathf.Min(progressDistance, _pullDistance);
            CurrentPullDistance = constrainedDistance;

            if (progressDistance >= _pullDistance)
            {
                DetachFromBodyIfNeeded();
                IsPinRemoved = true;
                _gravityEnabledAfterRelease = false;

                _whenPinRemoved?.Invoke();
            }
        }

        private void DetachFromBodyIfNeeded()
        {
            if (_hasDetachedFromBody)
            {
                return;
            }

            if (_originalParent != null)
            {
                transform.SetParent(null, true);
            }

            _hasDetachedFromBody = true;
        }

        private void EnsureFreePhysics()
        {
            if (_pinRigidbody == null)
            {
                return;
            }

            _pinRigidbody.isKinematic = false;
            _pinRigidbody.useGravity = true;
            _pinRigidbody.detectCollisions = true;
            _pinRigidbody.constraints = RigidbodyConstraints.None;
            _pinRigidbody.WakeUp();
        }

        private bool IsGrabInProgress()
        {
            if (_pinInteractable == null)
            {
                return false;
            }

            if (_indexOnlyGate != null && !_indexOnlyGate.HasValidSelectingInteractor)
            {
                return false;
            }

            foreach (GrabInteractor _ in _pinInteractable.SelectingInteractors)
            {
                return true;
            }

            return false;
        }

        private bool HasSelectingInteractor()
        {
            if (_pinInteractable == null)
            {
                return false;
            }

            foreach (GrabInteractor _ in _pinInteractable.SelectingInteractors)
            {
                return true;
            }

            return false;
        }

        private Vector3 GetAxisVector()
        {
            switch (_pullAxis)
            {
                case PullAxis.LocalRight:
                    return Vector3.right;
                case PullAxis.LocalForward:
                    return Vector3.forward;
                default:
                    return Vector3.up;
            }
        }

        private void ForceAxisOnlyPose(float axisDistance)
        {
            Vector3 clampedOffset = GetAxisWorld() * axisDistance;
            transform.position = GetStartWorldPosition() + clampedOffset;
            transform.rotation = GetStartWorldRotation();
        }

        private Vector3 GetStartWorldPosition()
        {
            return _pullReference.TransformPoint(_startLocalPosition);
        }

        private Quaternion GetStartWorldRotation()
        {
            return _pullReference.rotation * _startLocalRotation;
        }

        private Vector3 GetAxisWorld()
        {
            Vector3 localAxis = GetAxisVector();
            Vector3 worldAxis = _pullReference.TransformDirection(localAxis);
            return worldAxis.normalized;
        }
    }
}
