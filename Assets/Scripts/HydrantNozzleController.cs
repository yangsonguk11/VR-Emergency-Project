using EmergencyXR.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EmergencyXR
{
    public class HydrantNozzleController : MonoBehaviour
    {
        [Header("Valve Conditions")]
        [SerializeField] private ValveGrabTransformer _valveA;
        [SerializeField] private ValveGrabTransformer _valveB;

        [Tooltip("두 밸브 모두 이 각도 이상 열려야 분사됩니다.")]
        [SerializeField] private float _openAngleThreshold = 90f;

        [Header("Spray FX")]
        [SerializeField] private ParticleSystem _sprayParticles;
        [SerializeField] private AudioSource _sprayAudio;

        [Header("Events")]
        [SerializeField] private UnityEvent _whenFireStarted;
        [SerializeField] private UnityEvent _whenFireStopped;

        [Header("Debug")]
        [SerializeField] private bool _logFireState = true;
        [SerializeField] private bool _runtimeIsFiring;

        private bool _isFiring;

        public bool IsFiring => _isFiring;

        private void Update()
        {
            SetFiring(EvaluateCanFire());
            _runtimeIsFiring = _isFiring;
        }

        private bool EvaluateCanFire()
        {
            if (_valveA == null || _valveB == null)
                return false;

            return _valveA.CurrentAngle >= _openAngleThreshold
                && _valveB.CurrentAngle >= _openAngleThreshold;
        }

        private void SetFiring(bool shouldFire)
        {
            if (_isFiring == shouldFire)
                return;

            _isFiring = shouldFire;

            if (_logFireState)
                Debug.Log(_isFiring ? "발사 중" : "발사 중단", this);

            if (_isFiring)
                _whenFireStarted?.Invoke();
            else
                _whenFireStopped?.Invoke();

            if (_sprayParticles != null)
            {
                if (_isFiring && !_sprayParticles.isPlaying)
                    _sprayParticles.Play(true);
                else if (!_isFiring && _sprayParticles.isPlaying)
                    _sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (_sprayAudio != null)
            {
                if (_isFiring && !_sprayAudio.isPlaying)
                    _sprayAudio.Play();
                else if (!_isFiring && _sprayAudio.isPlaying)
                    _sprayAudio.Stop();
            }
        }
    }
}
