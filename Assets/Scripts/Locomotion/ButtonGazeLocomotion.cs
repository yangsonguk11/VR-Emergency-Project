using UnityEngine;

namespace EmergencyXR.Locomotion
{
    /// <summary>
    /// 왼손 Y 또는 오른손 B 버튼을 누르는 동안 시선 방향으로 이동합니다.
    /// </summary>
    public class ButtonGazeLocomotion : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private CharacterController _characterController;

        [SerializeField]
        private Transform _hmdTransform;

        [SerializeField]
        private ArmSwingSpeedBoost _armSwingBoost;

        [Tooltip("기존 조이스틱 이동 Provider가 있다면 여기 넣어 자동 비활성화하세요.")]
        [SerializeField]
        private Behaviour[] _disableWhileActive;

        [Header("Move")]
        [SerializeField]
        private float _baseMoveSpeed = 1.8f;

        [SerializeField]
        private bool _useUnscaledTime = false;

        [SerializeField]
        private bool _includeGravity = true;

        [SerializeField]
        private float _gravity = -9.81f;

        private float _verticalVelocity;
        private bool[] _previousProviderState;

        private void Reset()
        {
            if (_characterController == null)
            {
                _characterController = GetComponent<CharacterController>();
            }
        }

        private void Update()
        {
            if (_characterController == null || _hmdTransform == null)
            {
                return;
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
                Vector3 forwardFlat = _hmdTransform.forward;
                forwardFlat.y = 0f;
                if (forwardFlat.sqrMagnitude > 0.0001f)
                {
                    forwardFlat.Normalize();
                }
                else
                {
                    forwardFlat = Vector3.forward;
                }

                float speedMultiplier = _armSwingBoost != null ? _armSwingBoost.CurrentSpeedMultiplier : 1f;
                float currentSpeed = _baseMoveSpeed * speedMultiplier;
                planarVelocity = forwardFlat * currentSpeed;
            }

            if (_includeGravity)
            {
                if (_characterController.isGrounded && _verticalVelocity < 0f)
                {
                    _verticalVelocity = -1f;
                }
                else
                {
                    _verticalVelocity += _gravity * dt;
                }
            }
            else
            {
                _verticalVelocity = 0f;
            }

            Vector3 velocity = planarVelocity + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * dt);
        }

        private void OnEnable()
        {
            if (_disableWhileActive == null || _disableWhileActive.Length == 0)
            {
                return;
            }

            _previousProviderState = new bool[_disableWhileActive.Length];
            for (int i = 0; i < _disableWhileActive.Length; i++)
            {
                if (_disableWhileActive[i] == null)
                {
                    continue;
                }

                _previousProviderState[i] = _disableWhileActive[i].enabled;
                _disableWhileActive[i].enabled = false;
            }
        }

        private void OnDisable()
        {
            if (_disableWhileActive == null || _disableWhileActive.Length == 0 || _previousProviderState == null)
            {
                return;
            }

            for (int i = 0; i < _disableWhileActive.Length && i < _previousProviderState.Length; i++)
            {
                if (_disableWhileActive[i] == null)
                {
                    continue;
                }

                _disableWhileActive[i].enabled = _previousProviderState[i];
            }
        }

        private static bool IsMoveButtonPressed()
        {
            bool leftY = OVRInput.Get(OVRInput.RawButton.Y);
            bool rightB = OVRInput.Get(OVRInput.RawButton.B);
            return leftY || rightB;
        }
    }
}
