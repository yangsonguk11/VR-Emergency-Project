using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClickableTarget : MonoBehaviour
{
    public TargetType targetType;

    private GuideUIManager guideUIManager;

    void Start()
    {
        guideUIManager = FindObjectOfType<GuideUIManager>();
    }

    // 에디터 마우스 테스트용
    private void OnMouseDown()
    {
        Interact();
    }

    // VR Select Entered 이벤트용
    public void InteractFromXR(SelectEnterEventArgs args)
    {
        Interact();
    }

    private void Interact()
    {
        if (guideUIManager != null)
        {
            guideUIManager.OnTargetClicked(targetType, gameObject);
        }
    }
}