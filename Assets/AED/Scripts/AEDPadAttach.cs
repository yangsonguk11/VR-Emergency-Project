using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class AEDPadAttach : MonoBehaviour
{
    public Transform attachTarget;
    public float attachDistance = 0.12f;

    public Rigidbody padRigidbody;
    public Grabbable padGrabbable;
    public GrabInteractable padGrab;
    public HandGrabInteractable padHandGrab;

    private bool isAttached = false;

    private void Update()
    {
        if (isAttached)
            return;

        if (attachTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, attachTarget.position);

        if (distance <= attachDistance)
        {
            AttachPad();
        }
    }

    private void AttachPad()
    {
        isAttached = true;

        transform.position = attachTarget.position;
        transform.rotation = attachTarget.rotation;
        transform.SetParent(attachTarget);

        if (padRigidbody != null)
        {
            padRigidbody.linearVelocity = Vector3.zero;
            padRigidbody.angularVelocity = Vector3.zero;
            padRigidbody.isKinematic = true;
        }

        if (padGrabbable != null)
            padGrabbable.enabled = false;

        if (padGrab != null)
            padGrab.enabled = false;

        if (padHandGrab != null)
            padHandGrab.enabled = false;

        Debug.Log(gameObject.name + " 패드 부착 완료");
    }
}