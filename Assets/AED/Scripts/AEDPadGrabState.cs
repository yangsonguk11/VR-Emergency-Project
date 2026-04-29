using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class AEDPadGrabState : MonoBehaviour
{
    public Grabbable padGrabbable;

    public Grabbable aedGrabbable;
    public GrabInteractable aedGrab;
    public HandGrabInteractable aedHandGrab;

    private bool wasGrabbed = false;

    void Update()
    {
        if (padGrabbable == null) return;

        bool isGrabbed = padGrabbable.SelectingPointsCount > 0;

        // 잡힌 순간
        if (isGrabbed && !wasGrabbed)
        {
            OnPadGrabbed();
        }

        // 놓은 순간
        if (!isGrabbed && wasGrabbed)
        {
            OnPadReleased();
        }

        wasGrabbed = isGrabbed;
    }

    void OnPadGrabbed()
    {
        if (aedGrabbable != null) aedGrabbable.enabled = false;
        if (aedGrab != null) aedGrab.enabled = false;
        if (aedHandGrab != null) aedHandGrab.enabled = false;

        Debug.Log("패드 잡음 → AED Grab OFF");
    }

    void OnPadReleased()
    {
        if (aedGrabbable != null) aedGrabbable.enabled = true;
        if (aedGrab != null) aedGrab.enabled = true;
        if (aedHandGrab != null) aedHandGrab.enabled = true;

        Debug.Log("패드 놓음 → AED Grab ON");
    }
}