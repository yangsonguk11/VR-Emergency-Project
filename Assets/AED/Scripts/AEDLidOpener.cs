using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class AEDLidOpener : MonoBehaviour
{
    [Header("Lid")]
    public Transform lidObject;
    public float openAngle = 110f;
    public float speed = 120f;
    public Collider lidGrabCollider;

    [Header("Pad Green")]
    public Rigidbody padGreenRb;
    public Grabbable padGreenGrabbable;
    public GrabInteractable padGreenGrab;
    public HandGrabInteractable padGreenHandGrab;

    [Header("Pad Red")]
    public Rigidbody padRedRb;
    public Grabbable padRedGrabbable;
    public GrabInteractable padRedGrab;
    public HandGrabInteractable padRedHandGrab;

    private bool isOpening = false;
    private bool isOpened = false;
    private float currentAngle = 0f;

    private void Start()
    {
        // 시작할 때 패드 완전히 잠금
        SetPadsInteractable(false);
    }

    public void OpenLid()
    {
        if (isOpening || isOpened)
            return;

        Debug.Log("OpenLid 호출됨");
        isOpening = true;
    }

    private void Update()
    {
        if (!isOpening || isOpened)
            return;

        if (currentAngle < openAngle)
        {
            float delta = speed * Time.deltaTime;
            currentAngle += delta;

            if (currentAngle > openAngle)
                currentAngle = openAngle;

            if (lidObject != null)
                lidObject.localRotation = Quaternion.Euler(-currentAngle, 0f, 0f);
        }
        else
        {
            isOpened = true;

            // 뚜껑 잡기 Collider 끄기 (패드 방해 방지)
            if (lidGrabCollider != null)
                lidGrabCollider.enabled = false;

            // 패드 활성화
            SetPadsInteractable(true);

            Debug.Log("뚜껑 열림 완료 → 패드 활성화");
        }
    }

    private void SetPadsInteractable(bool active)
    {
        // Rigidbody 제어 (핵심)
        if (padGreenRb != null)
            padGreenRb.isKinematic = !active;

        if (padRedRb != null)
            padRedRb.isKinematic = !active;

        // Grab 제어
        if (padGreenGrabbable != null)
            padGreenGrabbable.enabled = active;

        if (padGreenGrab != null)
            padGreenGrab.enabled = active;

        if (padGreenHandGrab != null)
            padGreenHandGrab.enabled = active;

        if (padRedGrabbable != null)
            padRedGrabbable.enabled = active;

        if (padRedGrab != null)
            padRedGrab.enabled = active;

        if (padRedHandGrab != null)
            padRedHandGrab.enabled = active;
    }
}