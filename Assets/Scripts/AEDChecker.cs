using UnityEngine;

public class AEDChecker : MonoBehaviour
{
    [Header("Voice Manager")]
    public AEDVoiceGuideManager voiceGuideManager;

    [Header("Progress")]
    public int requiredPadCount = 2;

    private int attachedPadCount = 0;

    private bool lidOpened = false;
    private bool padsAttached = false;
    private bool shockButtonPressed = false;

    public GuideUIManager guideUIManager;

    public void NotifyLidOpened()
    {
        if (lidOpened) return;

        lidOpened = true;
        Debug.Log("AED 뚜껑 열림 완료");

        if (voiceGuideManager != null)
            voiceGuideManager.OnLidOpened();
    }

    public void NotifyPadAttached()
    {
        if (!lidOpened)
        {
            Debug.Log("뚜껑이 아직 안 열려서 패드 부착 무시");
            return;
        }

        if (padsAttached) return;

        attachedPadCount++;
        Debug.Log("패드 부착 수: " + attachedPadCount);

        if (attachedPadCount >= requiredPadCount)
        {
            padsAttached = true;
            Debug.Log("패드 2장 부착 완료");

            if (voiceGuideManager != null)
                voiceGuideManager.OnPadsAttached();
        }
    }

    public void NotifyShockButtonPressed()
    {
        if (!padsAttached)
        {
            Debug.Log("패드가 아직 2장 부착되지 않아서 제세동 버튼 무시");
            return;
        }

        if (shockButtonPressed) return;

        shockButtonPressed = true;
        Debug.Log("주황색 제세동 버튼 클릭 완료");

        if (voiceGuideManager != null)
            voiceGuideManager.OnShockButtonPressed();

        guideUIManager.OnAEDCompleted();
    }

    public bool IsLidOpened()
    {
        return lidOpened;
    }

    public bool ArePadsAttached()
    {
        return padsAttached;
    }

    public bool IsShockButtonPressed()
    {
        return shockButtonPressed;
    }
}