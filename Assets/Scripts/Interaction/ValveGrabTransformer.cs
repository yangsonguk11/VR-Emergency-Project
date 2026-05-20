using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EmergencyXR.Interaction
{
    /// <summary>
    /// Grabbable의 One Grab Transformer로 등록하면, 손의 움직임을
    /// 밸브의 단일 축 회전으로 변환합니다.
    /// 기존 Grabbable / GrabInteractable / HandGrabInteractable 수정 불필요.
    /// </summary>
    public class ValveGrabTransformer : MonoBehaviour, ITransformer
    {
        [Header("Rotation")]
        [Tooltip("회전 축 (로컬 공간 기준). 기본값은 Y축(위).")]
        [SerializeField] private Vector3 _rotationAxis = Vector3.up;

        [Tooltip("최소 회전 각도 (도). 이 값 이하로는 회전 불가.")]
        [SerializeField] private float _minAngle = 0f;

        [Tooltip("최대 회전 각도 (도). 이 값 이상으로는 회전 불가.")]
        [SerializeField] private float _maxAngle = 360f;

        [Header("Events")]
        [Tooltip("회전 각도가 변할 때마다 호출. 인자: 현재 누적 각도(도)")]
        [SerializeField] private UnityEvent<float> _onAngleChanged;

        [Tooltip("최솟값에 처음 도달했을 때 한 번 호출")]
        [SerializeField] private UnityEvent _onMinReached;

        [Tooltip("최댓값에 처음 도달했을 때 한 번 호출")]
        [SerializeField] private UnityEvent _onMaxReached;

        // ── 런타임 상태 ──────────────────────────────────────────────────
        private IGrabbable _grabbable;

        // Initialize 시점의 localRotation. 이 상태가 currentAngle = 0에 해당.
        private Quaternion _baseLocalRotation;

        // Initialize 시점에 고정된 월드 공간 회전 축.
        // 밸브가 돌아도 투영 기준이 흔들리지 않도록 캐싱.
        private Vector3 _worldAxis;

        // 이전 프레임의 손 투영 벡터 (프레임 간 델타 계산용)
        private Vector3 _prevHandProjected;

        // 초기 위치 기준으로 누적된 회전 각도
        private float _currentAngle;

        // 한계 도달 이벤트 중복 발생 방지 플래그
        private bool _minReachedFired;
        private bool _maxReachedFired;

        // ── 공개 읽기 전용 ────────────────────────────────────────────────
        /// <summary>현재 누적 회전 각도 (도)</summary>
        public float CurrentAngle => _currentAngle;

        /// <summary>설정된 최소 각도</summary>
        public float MinAngle => _minAngle;

        /// <summary>설정된 최대 각도</summary>
        public float MaxAngle => _maxAngle;

        // ── ITransformer 구현 ─────────────────────────────────────────────

        /// <summary>
        /// Grabbable.Start()에서 한 번 호출됩니다.
        /// 이 시점의 localRotation을 "0도" 기준으로 기억합니다.
        /// </summary>
        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
            _baseLocalRotation = grabbable.Transform.localRotation;
            // 초기화 시점의 월드 축을 고정 — 이후 밸브가 회전해도 투영 기준 불변
            _worldAxis = grabbable.Transform.TransformDirection(_rotationAxis).normalized;
        }

        /// <summary>
        /// 그랩 시작(또는 손 수 변경) 시 호출됩니다.
        /// 현재 손 위치를 기준점으로 캡처합니다.
        /// </summary>
        public void BeginTransform()
        {
            Debug.Log("ValveGrabTransformer: BeginTransform");
            _prevHandProjected = GetHandProjected();
        }

        /// <summary>
        /// 그랩 유지 중 매 프레임 호출됩니다.
        /// 이전 프레임 대비 손의 각도 변화량을 누적하여 밸브를 회전합니다.
        /// Min/Max 범위를 초과하면 회전이 멈춥니다.
        /// </summary>
        public void UpdateTransform()
        {
            Vector3 current = GetHandProjected();

            // 손이 밸브 중심과 너무 가까우면 계산 스킵
            if (current.sqrMagnitude < 0.0001f || _prevHandProjected.sqrMagnitude < 0.0001f)
            {
                _prevHandProjected = current;
                return;
            }

            // 프레임 간 회전 델타 (최대 ±180° — 한 프레임에 그 이상 움직이는 경우는 없음)
            float delta = Vector3.SignedAngle(_prevHandProjected, current, _worldAxis);
            _prevHandProjected = current;

            float newAngle = Mathf.Clamp(_currentAngle + delta, _minAngle, _maxAngle);

            if (Mathf.Approximately(newAngle, _currentAngle))
                return;

            _currentAngle = newAngle;

            // 회전 적용: 기준 localRotation에 누적 각도만큼 로컬 축 회전 추가
            _grabbable.Transform.localRotation =
                _baseLocalRotation * Quaternion.AngleAxis(_currentAngle, _rotationAxis);

            _onAngleChanged?.Invoke(_currentAngle);

            // 한계 도달 이벤트 (처음 도달할 때 한 번만)
            if (_currentAngle <= _minAngle && !_minReachedFired)
            {
                _minReachedFired = true;
                _maxReachedFired = false;
                _onMinReached?.Invoke();
            }
            else if (_currentAngle >= _maxAngle && !_maxReachedFired)
            {
                _maxReachedFired = true;
                _minReachedFired = false;
                _onMaxReached?.Invoke();
            }
            Debug.Log($"ValveGrabTransformer: UpdateTransform - CurrentAngle={_currentAngle:F2}°");
        }

        /// <summary>
        /// 그랩 해제 시 호출됩니다. 현재는 별도 처리 없음.
        /// </summary>
        public void EndTransform() { }

        // ── 내부 헬퍼 ────────────────────────────────────────────────────

        /// <summary>
        /// 손(그랩 포인트)의 위치를 회전 평면에 투영한 벡터를 반환합니다.
        /// 이 벡터의 방향 변화가 밸브 회전 델타가 됩니다.
        /// </summary>
        private Vector3 GetHandProjected()
        {
            if (_grabbable.GrabPoints.Count == 0)
                return Vector3.zero;

            Vector3 toHand = _grabbable.GrabPoints[0].position - _grabbable.Transform.position;
            return Vector3.ProjectOnPlane(toHand, _worldAxis);
        }
    }
}
