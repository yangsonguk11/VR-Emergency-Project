using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EmergencyXR.FireExtinguisher
{
    /// <summary>
    /// 손잡이 그랩 + 안전핀 분리 + 인덱스 트리거 입력 조건을 만족할 때 분사합니다.
    /// </summary>
    public class FireExtinguisherController : MonoBehaviour
    {
        [Header("Required References")]
        [SerializeField]
        private GrabInteractable _handleInteractable;

        [SerializeField]
        private GrabInteractor _leftGrabInteractor;

        [SerializeField]
        private GrabInteractor _rightGrabInteractor;

        [SerializeField]
        private SafetyPinPullConstraint _safetyPin;

        [Header("Spray FX")]
        [SerializeField]
        private ParticleSystem _sprayParticles;

        [SerializeField]
        private AudioSource _sprayAudio;

        [SerializeField]
        private Rigidbody _extinguisherBody;

        [SerializeField]
        private Transform _sprayNozzle;

        [Header("Firing")]
        [SerializeField, Range(0f, 1f)]
        private float _triggerThreshold = 0.65f;

        [SerializeField]
        private float _recoilForce = 2.5f;

        [Header("Test")]
        [SerializeField]
        private bool _logFireState = true;

        [SerializeField]
        private UnityEvent _whenFireStarted;

        [SerializeField]
        private UnityEvent _whenFireStopped;

        [SerializeField]
        private bool _runtimeIsFiring;

        private bool _isFiring;

        public bool IsHandleGrabbed => TryGetCurrentGrabInteractor(out _);
        public bool IsFiring => _isFiring;

        private void Update()
        {
            bool canFire = EvaluateCanFire();
            SetFiring(canFire);
            _runtimeIsFiring = _isFiring;

            if (_isFiring)
            {
                ApplyRecoil();
            }
        }

        private bool EvaluateCanFire()
        {
            if (_handleInteractable == null || _safetyPin == null)
            {
                return false;
            }

            if (!_safetyPin.IsPinRemoved)
            {
                return false;
            }

            if (!TryGetCurrentGrabInteractor(out GrabInteractor interactor))
            {
                return false;
            }

            OVRInput.Controller controller = ResolveController(interactor);
            if (controller == OVRInput.Controller.None)
            {
                return false;
            }

            float trigger = GetIndexTrigger(controller);
            if (trigger < _triggerThreshold)
            {
                return false;
            }

            return true;
        }

        private bool TryGetCurrentGrabInteractor(out GrabInteractor interactor)
        {
            foreach (GrabInteractor selectingInteractor in _handleInteractable.SelectingInteractors)
            {
                interactor = selectingInteractor;
                return true;
            }

            interactor = null;
            return false;
        }

        private OVRInput.Controller ResolveController(GrabInteractor interactor)
        {
            if (interactor == _leftGrabInteractor)
            {
                return OVRInput.Controller.LTouch;
            }

            if (interactor == _rightGrabInteractor)
            {
                return OVRInput.Controller.RTouch;
            }

            return OVRInput.Controller.None;
        }

        private static float GetIndexTrigger(OVRInput.Controller controller)
        {
            if ((controller & OVRInput.Controller.LTouch) != 0)
            {
                return OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            }

            if ((controller & OVRInput.Controller.RTouch) != 0)
            {
                return OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
            }

            return 0f;
        }

        private void SetFiring(bool shouldFire)
        {
            if (_isFiring == shouldFire)
            {
                return;
            }

            _isFiring = shouldFire;
            if (_logFireState)
            {
                Debug.Log(_isFiring ? "발사 중" : "발사 중단", this);
            }

            if (_isFiring)
            {
                _whenFireStarted?.Invoke();
            }
            else
            {
                _whenFireStopped?.Invoke();
            }

            if (_sprayParticles != null)
            {
                if (_isFiring && !_sprayParticles.isPlaying)
                {
                    _sprayParticles.Play(true);
                }
                else if (!_isFiring && _sprayParticles.isPlaying)
                {
                    _sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            if (_sprayAudio != null)
            {
                if (_isFiring && !_sprayAudio.isPlaying)
                {
                    _sprayAudio.Play();
                }
                else if (!_isFiring && _sprayAudio.isPlaying)
                {
                    _sprayAudio.Stop();
                }
            }
        }

        private void ApplyRecoil()
        {
            if (_extinguisherBody == null || _sprayNozzle == null || _recoilForce <= 0f)
            {
                return;
            }

            Vector3 direction = -_sprayNozzle.forward;
            _extinguisherBody.AddForceAtPosition(direction * _recoilForce, _sprayNozzle.position, ForceMode.Force);
        }

    }
}
