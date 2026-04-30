using UnityEngine;

public class AEDLidPressButton : MonoBehaviour
{
    public AEDLidOpener lidOpener;

    private bool isNearButton = false;
    private bool isOpened = false;

    private bool IsValidInteractor(Collider other)
    {
        string n = other.name.ToLower();

        return n.Contains("hand") ||
               n.Contains("controller") ||
               n.Contains("index") ||
               n.Contains("finger");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidInteractor(other))
            return;

        Debug.Log("LidPressZone 진입: " + other.name);
        isNearButton = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidInteractor(other))
            return;

        Debug.Log("LidPressZone 이탈: " + other.name);
        isNearButton = false;
    }

    private void Update()
    {
        if (isOpened) return;
        if (!isNearButton) return;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.One) ||
            OVRInput.GetDown(OVRInput.Button.Two))
        {
            Debug.Log("AED 버튼 입력 감지됨");

            isOpened = true;

            if (lidOpener != null)
                lidOpener.OpenLid();
            else
                Debug.Log("Lid Opener가 연결되지 않음");
        }
    }
}