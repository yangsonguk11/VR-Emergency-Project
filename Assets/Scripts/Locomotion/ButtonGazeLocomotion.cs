using UnityEngine;

namespace EmergencyXR.Locomotion
{
    /// <summary>
    /// 왼손 Y 또는 오른손 B 버튼을 누르는 동안 시선 방향으로 이동합니다.
    /// </summary>
    public class ButtonGazeLocomotion : MonoBehaviour
    {
        public enum MoveDirectionMode
        {
            HmdForward,
            ControllerForwardBlend
        }

        [Header("References")]
        [SerializeField]
        private Transform _hmdTransform;

        [SerializeField]
        private Transform _leftControllerTransform;

        [SerializeField]
        private Transform _rightControllerTransform;

        [SerializeField]
        private ArmSwingSpeedBoost _armSwingBoost;

        [SerializeField]
        private Transform _rigRoot;

        [Header("Move")]
        [SerializeField]
        private float _baseMoveSpeed = 1.8f;

        [SerializeField]
        private MoveDirectionMode _moveDirectionMode = MoveDirectionMode.HmdForward;

        [SerializeField]
        private bool _flattenDirectionOnHorizontalPlane = true;

        [SerializeField]
        private bool _useUnscaledTime = false;

        [SerializeField]
        private float _gravity = -9.81f;

        [SerializeField]
        private bool _useGravity = false;

        [Header("Collision")]
        [SerializeField]
        private bool _usePhysicsCollision = true;

        [SerializeField]
        private LayerMask _collisionLayers = ~0;

        [SerializeField]
        private float _collisionCapsuleHeight = 1.7f;

        [SerializeField]
        private float _collisionCapsuleRadius = 0.2f;

        [SerializeField]
        private float _collisionSkinWidth = 0.02f;

        private float _verticalVelocity;

        private void Reset()
        {
            if (_rigRoot == null)
            {
                _rigRoot = transform;
            }
        }

        private void Update()
        {
            if (_hmdTransform == null)
            {
                return;
            }

            if (_rigRoot == null)
            {
                _rigRoot = transform;
            }

            float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            bool movePressed = IsMoveButtonPressed();
            Vector3 planarVelocity = Vector3.zero;

            if (movePressed)
            {
                Vector3 moveDirection = ComputeMoveDirection();
                float speedMultiplier = _armSwingBoost != null ? _armSwingBoost.CurrentSpeedMultiplier : 1f;
                float currentSpeed = _baseMoveSpeed * speedMultiplier;
                planarVelocity = moveDirection * currentSpeed;
            }

            if (_useGravity)
            {
                _verticalVelocity += _gravity * dt;
            }
            else
            {
                _verticalVelocity = 0f;
            }

            Vector3 velocity = planarVelocity + Vector3.up * _verticalVelocity;
            Vector3 displacement = velocity * dt;
            if (_usePhysicsCollision)
            {
                displacement = ResolveCollisionDisplacement(_rigRoot.position, displacement);
            }

            _rigRoot.position += displacement;
        }

        private static bool IsMoveButtonPressed()
        {
            bool leftY = OVRInput.Get(OVRInput.RawButton.Y);
            bool rightB = OVRInput.Get(OVRInput.RawButton.B);
            return leftY || rightB;
        }

        private Vector3 ComputeMoveDirection()
        {
            Vector3 direction;
            switch (_moveDirectionMode)
            {
                case MoveDirectionMode.ControllerForwardBlend:
                    direction = ComputeControllerBlendDirection();
                    break;
                default:
                    direction = _hmdTransform.forward;
                    break;
            }

            if (_flattenDirectionOnHorizontalPlane)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            return direction;
        }

        private Vector3 ComputeControllerBlendDirection()
        {
            bool leftPressed = OVRInput.Get(OVRInput.RawButton.Y);
            bool rightPressed = OVRInput.Get(OVRInput.RawButton.B);

            Vector3 sum = Vector3.zero;
            if (leftPressed && _leftControllerTransform != null)
            {
                Vector3 leftForward = _leftControllerTransform.forward;
                leftForward.y = 0f;
                if (leftForward.sqrMagnitude > 0.0001f)
                {
                    sum += leftForward.normalized;
                }
            }

            if (rightPressed && _rightControllerTransform != null)
            {
                Vector3 rightForward = _rightControllerTransform.forward;
                rightForward.y = 0f;
                if (rightForward.sqrMagnitude > 0.0001f)
                {
                    sum += rightForward.normalized;
                }
            }

            // 둘 다 눌렀을 때 같은 방향이면 2배, 반대면 0에 가까워집니다.
            if (sum.sqrMagnitude > 0.0001f)
            {
                return sum;
            }

            return _hmdTransform.forward;
        }

        private Vector3 ResolveCollisionDisplacement(Vector3 origin, Vector3 desiredDisplacement)
        {
            float distance = desiredDisplacement.magnitude;
            if (distance < 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 direction = desiredDisplacement / distance;
            GetCapsulePoints(origin, out Vector3 p1, out Vector3 p2);

            if (!Physics.CapsuleCast(
                    p1, p2, _collisionCapsuleRadius, direction, out RaycastHit hit,
                    distance + _collisionSkinWidth, _collisionLayers, QueryTriggerInteraction.Ignore))
            {
                return desiredDisplacement;
            }

            float moveDistance = Mathf.Max(0f, hit.distance - _collisionSkinWidth);
            Vector3 moved = direction * moveDistance;

            // 한 번 슬라이드하여 벽면을 따라 자연스럽게 이동
            Vector3 remaining = desiredDisplacement - moved;
            Vector3 slide = Vector3.ProjectOnPlane(remaining, hit.normal);
            float slideDistance = slide.magnitude;
            if (slideDistance < 0.0001f)
            {
                return moved;
            }

            Vector3 slideDir = slide / slideDistance;
            Vector3 slideOrigin = origin + moved;
            GetCapsulePoints(slideOrigin, out Vector3 sp1, out Vector3 sp2);

            if (Physics.CapsuleCast(
                sp1, sp2, _collisionCapsuleRadius, slideDir, out RaycastHit slideHit,
                slideDistance + _collisionSkinWidth, _collisionLayers, QueryTriggerInteraction.Ignore))
            {
                float slideMoveDistance = Mathf.Max(0f, slideHit.distance - _collisionSkinWidth);
                return moved + slideDir * slideMoveDistance;
            }

            return moved + slide;
        }

        private void GetCapsulePoints(Vector3 rootPosition, out Vector3 point1, out Vector3 point2)
        {
            float radius = Mathf.Max(0.01f, _collisionCapsuleRadius);
            float height = Mathf.Max(radius * 2f + 0.01f, _collisionCapsuleHeight);
            float halfSegment = (height * 0.5f) - radius;
            Vector3 center = rootPosition + Vector3.up * (height * 0.5f);

            point1 = center + Vector3.up * halfSegment;
            point2 = center - Vector3.up * halfSegment;
        }
    }
}
