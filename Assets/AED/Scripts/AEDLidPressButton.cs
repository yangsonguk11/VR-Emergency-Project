using UnityEngine;

public class AEDLidPressButton : MonoBehaviour
{
    public AEDLidOpener lidOpener;

    private bool isNearButton = false;
    private bool isOpened = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("LidPressZone 진입: " + other.name);
        isNearButton = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("LidPressZone 이탈: " + other.name);
        isNearButton = false;
    }

    private void Update()
    {
        // 영역 안에 있는지 확인
        if (isNearButton)
        {
            Debug.Log("현재 버튼 영역 안에 있음");
        }

        if (isOpened) return;
        if (!isNearButton) return;

        // 버튼 입력 감지 (여러 개 다 체크)
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
                Debug.Log("Lid Opener가 연결되지 않음");
            }
        }
    }
}