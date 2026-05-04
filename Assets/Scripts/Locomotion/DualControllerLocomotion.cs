using UnityEngine;

namespace EmergencyXR.Locomotion
{
    /// <summary>
    /// 양쪽 컨트롤러가 가리키는 방향으로 이동합니다.
    /// 손을 앞뒤로 흔들면 흔드는 강도에 비례해 이동 속도가 증폭됩니다.
    /// CharacterController의 물리 폭발(튕겨나감)을 방지하기 위해 순수 Raycast/CapsuleCast 기반의 커스텀 물리로 구현되었습니다.
    /// 단차(계단)와 경사로를 자연스럽게 타고 오르며 중력이 적용됩니다.
    /// </summary>
    public class DualControllerLocomotion : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("HMD (카메라)의 Transform")]
        private Transform _hmdTransform;

        [SerializeField, Tooltip("왼쪽 컨트롤러 Transform")]
        private Transform _leftControllerTransform;

        [SerializeField, Tooltip("오른쪽 컨트롤러 Transform")]
        private Transform _rightControllerTransform;

        [Header("Movement Settings")]
        [SerializeField, Tooltip("각 컨트롤러당 기본 이동 속도 (동시 누름 시 2배)")]
        private float _speedPerController = 2.0f;

        [SerializeField, Tooltip("중력 가속도")]
        private float _gravity = -9.81f;
        
        [Header("Arm Swing Boost")]
        [SerializeField, Tooltip("팔 흔들기 인식 최소 속도 (이 속도 이상 흔들어야 배율 적용 시작)")]
        private float _swingActivationSpeed = 0.35f;

        [SerializeField, Tooltip("팔 흔들기 최대 인식 속도 (이 속도로 흔들면 최대 배율 도달)")]
        private float _swingFullSpeed = 1.4f;

        [SerializeField, Tooltip("최대로 흔들었을 때 곱해질 속도 배율 (예: 2 = 2배 속도)")]
        private float _maxSpeedMultiplier = 2.0f;

        [SerializeField, Range(0f, 1f), Tooltip("팔 위치 측정 평활도 (떨림 방지)")]
        private float _handPositionSmoothing = 0.25f;

        [Header("Collision & Environment")]
        [SerializeField, Tooltip("이동/충돌을 감지할 물리 레이어")]
        private LayerMask _environmentLayer = ~0;
        
        [SerializeField, Tooltip("플레이어 몸통 충돌 반경")] 
        private float _bodyRadius = 0.2f;
        
        [SerializeField, Tooltip("자연스럽게 오를 수 있는 계단/단차의 최대 높이")] 
        private float _stepHeight = 0.3f;
        
        [SerializeField, Tooltip("자연스럽게 오를 수 있는 최대 경사각")] 
        private float _slopeLimit = 45f;

        private float _verticalVelocity = 0f;
        private Collider[] _playerColliders;

        // Arm Swing 상태 변수
        private Vector3 _smoothedLeftLocalPos;
        private Vector3 _smoothedRightLocalPos;
        private Vector3 _lastLeftLocalPos;
        private Vector3 _lastRightLocalPos;
        private float _currentBoost01;
        private bool _isInitialized;

        private void Awake()
        {
            // 물리 폭발 방지용 CharacterController 강제 제거
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                Destroy(cc);
            }
        }

        private void Start()
        {
            _playerColliders = GetComponentsInChildren<Collider>(true);
        }

        private bool IsHitSelf(Collider hitCollider)
        {
            if (_playerColliders == null) return false;
            for (int i = 0; i < _playerColliders.Length; i++)
            {
                if (_playerColliders[i] == hitCollider) return true;
            }
            return false;
        }

        private void InitializeArmSwingState()
        {
            if (_hmdTransform != null && _leftControllerTransform != null && _rightControllerTransform != null)
            {
                _lastLeftLocalPos = _smoothedLeftLocalPos = _hmdTransform.InverseTransformPoint(_leftControllerTransform.position);
                _lastRightLocalPos = _smoothedRightLocalPos = _hmdTransform.InverseTransformPoint(_rightControllerTransform.position);
                _isInitialized = true;
            }
        }

        private void Update()
        {
            if (_hmdTransform == null || _leftControllerTransform == null || _rightControllerTransform == null) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (!_isInitialized)
            {
                InitializeArmSwingState();
            }

            // ==========================================
            // 1. Arm Swing 흔들기 속도 및 배율 계산
            // ==========================================
            float lerpT = 1f - Mathf.Pow(1f - Mathf.Clamp01(_handPositionSmoothing), dt * 60f);
            
            // HMD를 기준으로 컨트롤러의 로컬 위치를 구해, 플레이어 자체가 이동하는 속도의 영향을 제거
            Vector3 leftLocalPos = _hmdTransform.InverseTransformPoint(_leftControllerTransform.position);
            Vector3 rightLocalPos = _hmdTransform.InverseTransformPoint(_rightControllerTransform.position);

            _smoothedLeftLocalPos = Vector3.Lerp(_smoothedLeftLocalPos, leftLocalPos, lerpT);
            _smoothedRightLocalPos = Vector3.Lerp(_smoothedRightLocalPos, rightLocalPos, lerpT);

            Vector3 leftVelocity = (_smoothedLeftLocalPos - _lastLeftLocalPos) / dt;
            Vector3 rightVelocity = (_smoothedRightLocalPos - _lastRightLocalPos) / dt;

            _lastLeftLocalPos = _smoothedLeftLocalPos;
            _lastRightLocalPos = _smoothedRightLocalPos;

            // 상하 흔들림(y)은 제외하고 수평(xz) 흔들림만 계산
            leftVelocity.y = 0f;
            rightVelocity.y = 0f;
            float swingSpeed = leftVelocity.magnitude + rightVelocity.magnitude;

            // 흔드는 속도를 0~1 사이의 Boost 값으로 변환
            float targetBoost = Mathf.InverseLerp(_swingActivationSpeed, _swingFullSpeed, swingSpeed);
            _currentBoost01 = Mathf.MoveTowards(_currentBoost01, targetBoost, dt * 2.5f); // 2.5f는 배율이 오르내리는 속도

            // 최종 속도 배율
            float currentSpeedMultiplier = Mathf.Lerp(1f, _maxSpeedMultiplier, _currentBoost01);


            // ==========================================
            // 2. 컨트롤러 입력 기반 수평 이동 벡터 계산
            // ==========================================
            Vector3 inputMove = Vector3.zero;

            // 왼쪽 Y 버튼
            if (OVRInput.Get(OVRInput.RawButton.Y))
            {
                Vector3 leftDir = _leftControllerTransform.forward;
                leftDir.y = 0; // 수평 이동만
                if (leftDir.sqrMagnitude > 0.001f)
                    inputMove += leftDir.normalized * (_speedPerController * currentSpeedMultiplier);
            }

            // 오른쪽 B 버튼
            if (OVRInput.Get(OVRInput.RawButton.B))
            {
                Vector3 rightDir = _rightControllerTransform.forward;
                rightDir.y = 0; // 수평 이동만
                if (rightDir.sqrMagnitude > 0.001f)
                    inputMove += rightDir.normalized * (_speedPerController * currentSpeedMultiplier);
            }


            // ==========================================
            // 3. 수평 이동 처리 (단차 및 벽 충돌 포함)
            // ==========================================
            float currentHeight = _hmdTransform.position.y - transform.position.y;
            if (currentHeight < 0.2f) currentHeight = 1.5f;

            Vector3 horizontalDisplacement = Vector3.zero;

            if (inputMove.sqrMagnitude > 0.001f)
            {
                Vector3 desiredMove = inputMove * dt;
                Vector3 moveDir = desiredMove.normalized;
                float moveDist = desiredMove.magnitude;

                Vector3 pointBottom = new Vector3(_hmdTransform.position.x, transform.position.y + _stepHeight + _bodyRadius, _hmdTransform.position.z);
                Vector3 pointTop = new Vector3(_hmdTransform.position.x, transform.position.y + currentHeight - _bodyRadius, _hmdTransform.position.z);
                
                RaycastHit[] hits = Physics.CapsuleCastAll(pointBottom, pointTop, _bodyRadius, moveDir, moveDist + 0.05f, _environmentLayer);
                
                bool hitWall = false;
                RaycastHit closestHit = new RaycastHit();
                float minDistance = float.MaxValue;

                foreach (var hit in hits)
                {
                    if (IsHitSelf(hit.collider) || hit.collider.isTrigger) continue;
                    
                    if (hit.distance < minDistance)
                    {
                        minDistance = hit.distance;
                        closestHit = hit;
                        hitWall = true;
                    }
                }

                if (hitWall)
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, closestHit.normal);
                    
                    if (slopeAngle <= _slopeLimit)
                    {
                        // 완만한 경사로
                        Vector3 slideDir = Vector3.ProjectOnPlane(moveDir, closestHit.normal).normalized;
                        horizontalDisplacement = slideDir * moveDist;
                    }
                    else
                    {
                        // 가파른 벽 미끄러짐
                        Vector3 slideDir = Vector3.ProjectOnPlane(moveDir, closestHit.normal);
                        slideDir.y = 0; 
                        if (slideDir.sqrMagnitude > 0.001f)
                        {
                            horizontalDisplacement = slideDir.normalized * moveDist;
                        }
                    }
                }
                else
                {
                    horizontalDisplacement = desiredMove;
                }
            }


            // ==========================================
            // 4. 중력 및 바닥/단차 충돌 (덜덜거림 방지)
            // ==========================================
            float verticalDisplacement = 0f;
            
            Vector3 expectedHmdPos = _hmdTransform.position + horizontalDisplacement;
            Vector3 rayOrigin = new Vector3(expectedHmdPos.x, transform.position.y + _stepHeight + 0.1f, expectedHmdPos.z);
            float castDistance = _stepHeight + 0.2f;
            
            RaycastHit[] floorHits = Physics.RaycastAll(rayOrigin, Vector3.down, castDistance, _environmentLayer);
            
            bool isGrounded = false;
            RaycastHit closestFloor = new RaycastHit();
            float minFloorDist = float.MaxValue;

            foreach (var hit in floorHits)
            {
                if (IsHitSelf(hit.collider) || hit.collider.isTrigger) continue;
                
                if (hit.distance < minFloorDist)
                {
                    minFloorDist = hit.distance;
                    closestFloor = hit;
                    isGrounded = true;
                }
            }


            // ==========================================
            // 5. 최종 위치 적용
            // ==========================================
            if (isGrounded)
            {
                _verticalVelocity = 0f;
                float floorY = closestFloor.point.y;
                
                Vector3 finalPos = transform.position + horizontalDisplacement;
                finalPos.y = floorY;
                transform.position = finalPos;
            }
            else
            {
                _verticalVelocity += _gravity * dt;
                verticalDisplacement = _verticalVelocity * dt;
                
                transform.position += horizontalDisplacement + Vector3.up * verticalDisplacement;
            }
        }
    }
}