using UnityEngine;

public class AEDLidPressButton : MonoBehaviour
{
    public AEDLidOpener lidOpener;

    [Header("Trigger Settings")]
    public string handTag = "PlayerHand";

    private bool isNearButton = false;
    private bool isOpened = false;

    private void OnTriggerEnter(Collider other)
    {
        // PlayerHand 태그가 아닌 오브젝트는 무시
        if (!other.CompareTag(handTag))
            return;

        Debug.Log("LidPressZone 진입: " + other.name);
        isNearButton = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // PlayerHand 태그가 아닌 오브젝트는 무시
        if (!other.CompareTag(handTag))
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
            Debug.Log("컨트롤러 버튼 입력 감지됨");

            isOpened = true;

            if (lidOpener != null)
            {
                lidOpener.OpenLid();
            }
            else
            {
                Debug.LogError("Lid Opener가 연결되지 않음");
            }
        }
    }

    public void PressButton()
    {
        if (isOpened) return;

        Debug.Log("PressButton 실행됨");

        isOpened = true;

        if (lidOpener != null)
        {
            lidOpener.OpenLid();
        }
    }
}