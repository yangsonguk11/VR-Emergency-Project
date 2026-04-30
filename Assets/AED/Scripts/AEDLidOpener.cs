using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class AEDLidOpener : MonoBehaviour
{
    [Header("Lid")]
    public Transform lidObject;
    public Vector3 closedRotation = new Vector3(-180f, 0f, 0f);
    public float openAngle = 110f;
    public float speed = 120f;
    public Collider lidGrabCollider;
    public Collider lidCollider;

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
        currentAngle = 0f;
        isOpening = false;
        isOpened = false;

        if (lidObject != null)
            lidObject.localRotation = Quaternion.Euler(closedRotation);

        if (lidCollider != null)
            lidCollider.enabled = true;

        if (lidGrabCollider != null)
            lidGrabCollider.enabled = true;

        SetPadsInteractable(false);
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
            {
                lidObject.localRotation = Quaternion.Euler(
                    closedRotation.x - currentAngle,
                    closedRotation.y,
                    closedRotation.z
                );
            }
        }
        else
        {
            isOpened = true;

            if (lidGrabCollider != null)
                lidGrabCollider.enabled = false;

            if (lidCollider != null)
                lidCollider.enabled = false;

            SetPadsInteractable(true);

            Debug.Log("¶Ñ²± ¿­¸² ¿Ï·á ¡æ ÆÐµå È°¼ºÈ­");
        }
    }

    public void OpenLid()
    {
        if (isOpened || isOpening)
            return;

        Debug.Log("OpenLid È£ÃâµÊ: " + Time.time);

        isOpening = true;
    }

    private void SetPadsInteractable(bool active)
    {
        if (padGreenRb != null)
            padGreenRb.isKinematic = !active;

        if (padRedRb != null)
            padRedRb.isKinematic = !active;

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