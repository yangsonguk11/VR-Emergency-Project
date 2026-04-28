using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class AEDPadAttach : MonoBehaviour
{
    [Header("Attach Target")]
    public Transform attachTarget;
    public Collider bodyCollider;
    public float attachDistance = 0.12f;
    public float surfaceOffset = 0.01f;

    [Header("Pad Components")]
    public Rigidbody padRigidbody;
    public Grabbable padGrabbable;
    public GrabInteractable padGrab;
    public HandGrabInteractable padHandGrab;

    [Header("Rotation")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    private bool isAttached = false;

    private void Update()
    {
        if (isAttached)
            return;

        if (attachTarget == null || bodyCollider == null)
            return;

        if (padRigidbody != null)
        {
            padRigidbody.linearVelocity *= 0.85f;
            padRigidbody.angularVelocity *= 0.85f;
        }

        float distanceToTarget = Vector3.Distance(transform.position, attachTarget.position);

        if (distanceToTarget <= attachDistance)
        {
            Vector3 surfacePoint = bodyCollider.ClosestPoint(attachTarget.position);
            AttachToBody(surfacePoint);
        }
    }

    private void AttachToBody(Vector3 surfacePoint)
    {
        isAttached = true;

        Vector3 outwardDirection = (attachTarget.position - bodyCollider.transform.position).normalized;

        if (outwardDirection == Vector3.zero)
            outwardDirection = bodyCollider.transform.forward;

        transform.position = surfacePoint + outwardDirection * surfaceOffset;

        Quaternion surfaceRotation = Quaternion.LookRotation(outwardDirection);
        transform.rotation = surfaceRotation * Quaternion.Euler(rotationOffset);

        transform.SetParent(bodyCollider.transform);

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