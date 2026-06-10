using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class AEDPadAttach : MonoBehaviour
{
    [Header("Attach Target")]
    public Transform attachTarget;
    public Collider bodyCollider;
    public float attachDistance = 0.25f;
    public float surfaceOffset = 0.03f;

    [Header("Pad Components")]
    public Rigidbody padRigidbody;
    public Grabbable padGrabbable;
    public GrabInteractable padGrab;
    public HandGrabInteractable padHandGrab;

    [Header("Rotation")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    private bool isAttached = false;
    private bool hasBeenGrabbed = false;

    public AEDChecker attachChecker;

    private void Update()
    {
        // 이미 부착되었으면 종료
        if (isAttached)
        {
            Debug.Log(gameObject.name + " 이미 부착됨");
            return;
        }

        // VR Grab 체크
        if (padGrabbable != null && padGrabbable.SelectingPointsCount > 0)
        {
            hasBeenGrabbed = true;
            Debug.Log(gameObject.name + " VR로 잡힘");
        }

        // 아직 안 잡힌 상태
        if (!hasBeenGrabbed)
        {
            Debug.Log(gameObject.name + " 아직 안잡힘");
            return;
        }

        // 타겟 체크
        if (attachTarget == null)
        {
            Debug.Log("attachTarget 없음");
            return;
        }

        if (bodyCollider == null)
        {
            Debug.Log("bodyCollider 없음");
            return;
        }

        // 속도 감소
        if (padRigidbody != null)
        {
            padRigidbody.linearVelocity *= 0.85f;
            padRigidbody.angularVelocity *= 0.85f;
        }

        // 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, attachTarget.position);

        Debug.Log(gameObject.name + " 현재 거리: " + distanceToTarget);

        // 부착 거리 안에 들어옴
        if (distanceToTarget <= attachDistance)
        {
            Debug.Log(gameObject.name + " 부착 거리 도달");

            AttachToBody();
        }
    }

    public void MarkGrabbedForEditor()
    {
        hasBeenGrabbed = true;
        Debug.Log(gameObject.name + " 에디터 드래그 Grab 처리됨");
    }

    private void AttachToBody()
    {
        Debug.Log(gameObject.name + " AttachToBody 실행");

        isAttached = true;

        if (padRigidbody != null)
        {
            padRigidbody.linearVelocity = Vector3.zero;
            padRigidbody.angularVelocity = Vector3.zero;
            padRigidbody.isKinematic = true;
        }

        // 환자 몸 표면 위치에 약간 띄워서 부착
        transform.position = attachTarget.position;

        // attachTarget의 회전에 보정값을 더해서 부착
        transform.rotation = attachTarget.rotation * Quaternion.Euler(rotationOffset);

        transform.SetParent(bodyCollider.transform, true);

        if (padGrabbable != null)
            padGrabbable.enabled = false;

        if (padGrab != null)
            padGrab.enabled = false;

        if (padHandGrab != null)
            padHandGrab.enabled = false;

        Debug.Log(gameObject.name + " 패드 부착 완료");

        if (attachChecker != null)
        {
            attachChecker.NotifyPadAttached();
        }
    }

    public bool IsAttached()
    {
        return isAttached;
    }
}