using UnityEngine;

public class ClickableTarget : MonoBehaviour
{
    public TargetType targetType;

    private GuideUIManager guideUIManager;

    private void Start()
    {
        guideUIManager = FindObjectOfType<GuideUIManager>();
    }

    // PC 테스트용
    private void OnMouseDown()
    {
        Interact();
    }

    public void Interact()
    {
        if (guideUIManager != null)
        {
            guideUIManager.OnTargetClicked(targetType, gameObject);
        }
    }
}