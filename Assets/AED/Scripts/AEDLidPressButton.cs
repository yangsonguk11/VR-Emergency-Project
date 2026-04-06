using UnityEngine;

public class AEDLidPressButton : MonoBehaviour
{
    public AEDLidOpener lidOpener;
    private bool pressed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (pressed)
            return;

        pressed = true;

        if (lidOpener != null)
            lidOpener.OpenLid();

        Debug.Log("È¨ ¹öÆ° ´­¸² -> ¶Ñ²± ¿­±â");
    }
}