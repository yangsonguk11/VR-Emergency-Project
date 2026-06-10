using UnityEngine;

public class AEDStartButton : MonoBehaviour
{
    [Header("AED Checker")]
    public AEDChecker aedChecker;

    private bool isPressed = false;

    public void Interact()
    {
        PressButton();
    }

    public void PressButton()
    {
        Debug.Log("AED 실행 버튼 PressButton 실행");

        if (isPressed)
        {
            Debug.Log("이미 버튼이 눌렸습니다.");
            return;
        }

        if (aedChecker == null)
        {
            Debug.LogError("AEDChecker가 연결되지 않았습니다.");
            return;
        }

        isPressed = true;

        aedChecker.NotifyShockButtonPressed();

        Debug.Log("AED 실행 버튼 클릭 성공");
    }
}