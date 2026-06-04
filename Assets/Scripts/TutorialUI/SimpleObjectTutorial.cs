using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EmergencyXR.Tutorial
{
    public enum TutorialDetectionMode
    {
        /// <summary>GrabInteractable의 Hover를 감지합니다. 잡을 수 있는 오브젝트(소화기, 노즐 등)에 사용합니다.</summary>
        GrabHover,
        /// <summary>트리거 콜리더로 범위 진입을 감지합니다. 접근만 해도 되는 경우(화재 위치, 문 등)에 사용합니다.</summary>
        ProximityTrigger
    }

    public class SimpleObjectTutorial : MonoBehaviour
    {
        [Header("Detection Mode")]
        public TutorialDetectionMode detectionMode = TutorialDetectionMode.GrabHover;

        [Header("GrabHover 설정 (detectionMode = GrabHover 일 때)")]
        [Tooltip("Hover 감지에 사용할 GrabInteractable. 비워두면 자식에서 자동으로 찾습니다.")]
        [SerializeField] private GrabInteractable _grabInteractable;

        [Header("ProximityTrigger 설정 (detectionMode = ProximityTrigger 일 때)")]
        [Tooltip("체크하면 GrabInteractor가 붙은 컨트롤러/손만 감지합니다. 해제하면 모든 콜리더에 반응합니다.")]
        public bool grabInteractorOnly = true;

        [Header("UI Settings")]
        [Tooltip("띄울 UI 프리팹 (World Space Canvas)")]
        public GameObject uiPrefab;

        [Tooltip("UI가 생성될 기준 위치. 비워두면 이 오브젝트 위치를 사용합니다.")]
        public Transform uiAnchor;

        [Tooltip("앵커로부터의 오프셋")]
        public Vector3 uiOffset = new Vector3(0, 0.3f, 0);

        [Header("Events")]
        public UnityEvent onTutorialShown;
        public UnityEvent onTutorialHidden;
        public UnityEvent onTutorialCompleted;

        private GameObject _uiInstance;
        private bool _isCompleted = false;
        private int _hoverCount = 0;

        private void Awake()
        {
            if (uiAnchor == null)
                uiAnchor = transform;

            if (detectionMode == TutorialDetectionMode.GrabHover && _grabInteractable == null)
                _grabInteractable = GetComponentInChildren<GrabInteractable>();
        }

        private void OnEnable()
        {
            if (detectionMode != TutorialDetectionMode.GrabHover || _grabInteractable == null) return;

            _grabInteractable.WhenInteractorViewAdded += OnInteractorEntered;
            _grabInteractable.WhenInteractorViewRemoved += OnInteractorExited;
            _grabInteractable.WhenSelectingInteractorViewAdded += OnGrabbed;
            _grabInteractable.WhenSelectingInteractorViewRemoved += OnReleased;
        }

        private void OnDisable()
        {
            if (detectionMode != TutorialDetectionMode.GrabHover || _grabInteractable == null) return;

            _grabInteractable.WhenInteractorViewAdded -= OnInteractorEntered;
            _grabInteractable.WhenInteractorViewRemoved -= OnInteractorExited;
            _grabInteractable.WhenSelectingInteractorViewAdded -= OnGrabbed;
            _grabInteractable.WhenSelectingInteractorViewRemoved -= OnReleased;
        }

        // GrabHover 방식 - 접근/이탈
        private void OnInteractorEntered(IInteractorView interactor)
        {
            if (_isCompleted) return;
            _hoverCount++;
            // 잡고 있는 중이 아닐 때만 UI 표시
            if (_hoverCount == 1 && !IsGrabbed()) ShowUI();
        }

        private void OnInteractorExited(IInteractorView interactor)
        {
            if (_isCompleted) return;
            _hoverCount = Mathf.Max(0, _hoverCount - 1);
            if (_hoverCount == 0) HideUI();
        }

        // GrabHover 방식 - 잡음/놓음
        private void OnGrabbed(IInteractorView interactor)
        {
            HideUI();
        }

        private void OnReleased(IInteractorView interactor)
        {
            if (_isCompleted) return;
            // 놓은 후에도 hover 범위 안에 있으면 다시 표시
            if (_hoverCount > 0 && !IsGrabbed()) ShowUI();
        }

        private bool IsGrabbed()
        {
            return _grabInteractable != null && _grabInteractable.State == InteractableState.Select;
        }

        // ProximityTrigger 방식
        private void OnTriggerEnter(Collider other)
        {
            if (_isCompleted || detectionMode != TutorialDetectionMode.ProximityTrigger) return;
            if (IsValidTrigger(other)) ShowUI();
        }

        private void OnTriggerExit(Collider other)
        {
            if (_isCompleted || detectionMode != TutorialDetectionMode.ProximityTrigger) return;
            if (IsValidTrigger(other)) HideUI();
        }

        private bool IsValidTrigger(Collider other)
        {
            if (!grabInteractorOnly) return true;
            return other.GetComponentInParent<GrabInteractor>() != null;
        }

        private void ShowUI()
        {
            if (_uiInstance != null || uiPrefab == null) return;
            _uiInstance = Instantiate(uiPrefab, uiAnchor);
            _uiInstance.transform.localPosition = uiOffset;
            onTutorialShown?.Invoke();
        }

        private void HideUI()
        {
            if (_uiInstance == null) return;
            Destroy(_uiInstance);
            _uiInstance = null;
            onTutorialHidden?.Invoke();
        }

        /// <summary>외부 이벤트에 연결해서 튜토리얼을 영구 종료합니다.</summary>
        public void CompleteTutorial()
        {
            if (_isCompleted) return;
            _isCompleted = true;
            HideUI();
            onTutorialCompleted?.Invoke();
        }

        /// <summary>튜토리얼을 초기 상태로 되돌립니다.</summary>
        public void ResetTutorial()
        {
            _isCompleted = false;
            _hoverCount = 0;
        }
    }
}

