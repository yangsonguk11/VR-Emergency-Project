using Oculus.Interaction;
using UnityEngine;

public class NozzleGrabController : MonoBehaviour
{
    [SerializeField] GrabInteractable _grabInteractable;
    private Rigidbody[] _rigidbodies;
    private bool _wasGrabbed;

    private void Awake()
    {
        // 루트 Rigidbody를 제외한 자식 Rigidbody들만 저장
        Rigidbody rootRb = GetComponent<Rigidbody>();
        Rigidbody[] allRbs = GetComponentsInChildren<Rigidbody>();

        System.Collections.Generic.List<Rigidbody> childRbs =
            new System.Collections.Generic.List<Rigidbody>(allRbs.Length);
        foreach (var rb in allRbs)
        {
            if (rb != rootRb)
                childRbs.Add(rb);
        }
        _rigidbodies = childRbs.ToArray();
    }

    private void Start()
    {
        // 초기 상태: 모든 Rigidbody를 kinematic으로 설정
        SetKinematic(true);
    }

    private void Update()
    {
        if(_wasGrabbed)
            return;
        bool isCurrentlyGrabbed = _grabInteractable.SelectingInteractors.Count > 0;

        // 상태 변경 시에만 업데이트
        if (isCurrentlyGrabbed)
        {
            SetKinematic(!isCurrentlyGrabbed);
            _wasGrabbed = true;
        }
    }

    private void SetKinematic(bool isKinematic)
    {
        foreach (Rigidbody rb in _rigidbodies)
        {
            rb.isKinematic = isKinematic;
        }
    }
}
