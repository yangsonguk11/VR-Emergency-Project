using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

namespace EmergencyXR.Interaction
{
    /// <summary>
    /// 컨트롤러 그랩 중 인덱스 트리거 조건을 만족할 때만 그랩을 유지합니다.
    /// </summary>
    [RequireComponent(typeof(GrabInteractable))]
    public class IndexOnlyGrabbable : MonoBehaviour
    {
        [SerializeField]
        private GrabInteractable _interactable;

        [Header("Interactor Mapping (권장)")]
        [SerializeField]
        private GrabInteractor _leftGrabInteractor;

        [SerializeField]
        private GrabInteractor _rightGrabInteractor;

        [Header("Controller Trigger Thresholds")]
        [SerializeField, Range(0f, 1f)]
        private float _minIndexTrigger = 0.55f;

        [SerializeField, Range(0f, 1f)]
        private float _maxHandTrigger = 0.95f;

        [SerializeField]
        private bool _requireLowHandTrigger = false;

        [Header("Index-Only Select Mode")]
        [SerializeField]
        private bool _forceSelectWithIndexTrigger = true;

        [SerializeField, Range(0f, 1f)]
        private float _releaseIndexTrigger = 0.2f;

        [Header("Debug")]
        [SerializeField]
        private bool _debugLogs = false;

        [SerializeField]
        private bool _logPointerEvents = true;

        [SerializeField]
        private string _lastDebugMessage;

        [SerializeField]
        private float _lastIndexTriggerValue;

        [SerializeField]
        private float _lastHandTriggerValue;

        private readonly List<int> _releaseBuffer = new List<int>(2);

        public bool HasValidSelectingInteractor
        {
            get
            {
                if (_interactable == null)
                {
                    return false;
                }

                foreach (GrabInteractor interactor in _interactable.SelectingInteractors)
                {
                    if (MeetsControllerCondition(interactor))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Reset()
        {
            _interactable = GetComponent<GrabInteractable>();
        }

        private void Awake()
        {
            if (_interactable == null)
            {
                _interactable = GetComponent<GrabInteractable>();
            }

            if (_interactable != null)
            {
                _interactable.WhenPointerEventRaised += HandlePointerEventRaised;
            }
        }

        private void OnDestroy()
        {
            if (_interactable != null)
            {
                _interactable.WhenPointerEventRaised -= HandlePointerEventRaised;
            }
        }

        private void LateUpdate()
        {
            if (_forceSelectWithIndexTrigger)
            {
                TryForceSelectOrRelease(_leftGrabInteractor, OVRInput.Controller.LTouch);
                TryForceSelectOrRelease(_rightGrabInteractor, OVRInput.Controller.RTouch);
            }

            _releaseBuffer.Clear();

            foreach (GrabInteractor interactor in _interactable.SelectingInteractors)
            {
                if (!MeetsControllerCondition(interactor))
                {
                    _releaseBuffer.Add(interactor.Identifier);
                }
            }

            for (int i = 0; i < _releaseBuffer.Count; i++)
            {
                _interactable.RemoveInteractorByIdentifier(_releaseBuffer[i]);
            }
        }

        private bool MeetsControllerCondition(GrabInteractor interactor)
        {
            if (interactor == null)
            {
                _lastDebugMessage = "Reject: interactor is null";
                return false;
            }

            OVRInput.Controller controller = ResolveController(interactor);
            if (controller == OVRInput.Controller.None)
            {
                // 인터랙터 매핑이 없으면 강제 해제하지 않음(오배정 방지)
                _lastDebugMessage = "Pass: interactor mapping unresolved";
                return true;
            }

            float index = GetIndexTrigger(controller);
            float hand = GetHandTrigger(controller);
            _lastIndexTriggerValue = index;
            _lastHandTriggerValue = hand;

            if (_requireLowHandTrigger && hand > _maxHandTrigger)
            {
                _lastDebugMessage = $"Reject: handTrigger({hand:0.00}) > max({_maxHandTrigger:0.00})";
                if (_debugLogs)
                {
                    Debug.Log($"[IndexOnlyGrabbable:{name}] {_lastDebugMessage}", this);
                }
                return false;
            }

            bool pass = index >= _minIndexTrigger;
            _lastDebugMessage = pass
                ? $"Pass: indexTrigger({index:0.00}) >= min({_minIndexTrigger:0.00})"
                : $"Reject: indexTrigger({index:0.00}) < min({_minIndexTrigger:0.00})";

            if (_debugLogs && !pass)
            {
                Debug.Log($"[IndexOnlyGrabbable:{name}] {_lastDebugMessage}", this);
            }

            return pass;
        }

        private void TryForceSelectOrRelease(GrabInteractor interactor, OVRInput.Controller controller)
        {
            if (interactor == null || _interactable == null)
            {
                return;
            }

            float index = GetIndexTrigger(controller);
            bool wantsSelect = index >= _minIndexTrigger;
            bool wantsRelease = index <= _releaseIndexTrigger;

            bool isThisSelected = interactor.SelectedInteractable == _interactable;

            if (wantsSelect && !isThisSelected)
            {
                bool canSelectTarget =
                    interactor.Interactable == _interactable ||
                    interactor.Candidate == _interactable;

                if (canSelectTarget)
                {
                    interactor.ForceSelect(_interactable);
                    if (_debugLogs)
                    {
                        Debug.Log($"[IndexOnlyGrabbable:{name}] ForceSelect by {controller} index={index:0.00}", this);
                    }
                }
                else
                {
                    _lastDebugMessage = $"Waiting candidate/hover for {controller}";
                }
            }
            else if (wantsRelease && isThisSelected)
            {
                interactor.ForceRelease();
                if (_debugLogs)
                {
                    Debug.Log($"[IndexOnlyGrabbable:{name}] ForceRelease by {controller} index={index:0.00}", this);
                }
            }
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
            if (controller == OVRInput.Controller.LTouch)
            {
                return OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            }

            if (controller == OVRInput.Controller.RTouch)
            {
                return OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
            }

            return 0f;
        }

        private static float GetHandTrigger(OVRInput.Controller controller)
        {
            if (controller == OVRInput.Controller.LTouch)
            {
                return OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
            }

            if (controller == OVRInput.Controller.RTouch)
            {
                return OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
            }

            return 0f;
        }

        private void HandlePointerEventRaised(PointerEvent evt)
        {
            if (!_debugLogs || !_logPointerEvents)
            {
                return;
            }

            Debug.Log($"[IndexOnlyGrabbable:{name}] PointerEvent={evt.Type}, InteractorId={evt.Identifier}", this);
        }
    }
}
