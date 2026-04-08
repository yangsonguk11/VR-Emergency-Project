using UnityEngine;

namespace EmergencyXR.Debugging
{
    /// <summary>
    /// 컨트롤러 트리거 입력이 실제로 들어오는지 확인하는 임시 디버그 컴포넌트입니다.
    /// </summary>
    public class ControllerTriggerDebug : MonoBehaviour
    {
        [SerializeField]
        private bool _logContinuously = false;

        [SerializeField]
        private bool _showOnScreen = true;

        [SerializeField, Range(0f, 1f)]
        private float _changeThreshold = 0.02f;

        [SerializeField]
        private float _logInterval = 0.2f;

        private float _lastLIndex;
        private float _lastRIndex;
        private float _lastLHand;
        private float _lastRHand;
        private float _nextLogTime;
        private string _cachedText = string.Empty;

        private void Update()
        {
            float lIndex = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            float rIndex = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
            float lHand = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
            float rHand = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

            bool changed =
                Mathf.Abs(lIndex - _lastLIndex) > _changeThreshold ||
                Mathf.Abs(rIndex - _lastRIndex) > _changeThreshold ||
                Mathf.Abs(lHand - _lastLHand) > _changeThreshold ||
                Mathf.Abs(rHand - _lastRHand) > _changeThreshold;

            _cachedText =
                $"LIndex: {lIndex:0.00} | RIndex: {rIndex:0.00}\n" +
                $"LHand : {lHand:0.00} | RHand : {rHand:0.00}\n" +
                $"Y: {OVRInput.Get(OVRInput.RawButton.Y)} | B: {OVRInput.Get(OVRInput.RawButton.B)}";

            if ((_logContinuously || changed) && Time.unscaledTime >= _nextLogTime)
            {
                Debug.Log($"[ControllerTriggerDebug] {_cachedText}", this);
                _nextLogTime = Time.unscaledTime + Mathf.Max(0.01f, _logInterval);
            }

            _lastLIndex = lIndex;
            _lastRIndex = rIndex;
            _lastLHand = lHand;
            _lastRHand = rHand;
        }

        private void OnGUI()
        {
            if (!_showOnScreen)
            {
                return;
            }

            GUI.Label(new Rect(20f, 20f, 420f, 90f), _cachedText);
        }
    }
}
