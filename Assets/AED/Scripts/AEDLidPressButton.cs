using UnityEngine;

public class AEDLidPressButton : MonoBehaviour
{
    public AEDLidOpener lidOpener;

    private bool isInside = false;
    private bool pressed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Controller") || other.CompareTag("Hand"))
        {
            isInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Controller") || other.CompareTag("Hand"))
        {
            isInside = false;
        }
    }

    private void Update()
    {
        if (pressed) return;
        if (!isInside) return;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            pressed = true;

            if (lidOpener != null)
            {
                lidOpener.OpenLid();
            }

            Debug.Log("È¨ ¹öÆ° ´­¸² -> ¶Ñ²± ¿­±â");
        }
    }
}