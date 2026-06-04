using UnityEngine;

public class PatientAreaTrigger : MonoBehaviour
{
    public GuideUIManager guideUIManager;

    void OnTriggerEnter(Collider other)
    {
        // Player 태그를 가진 오브젝트가 들어왔는지 확인
        if (other.CompareTag("Player"))
        {
            if (guideUIManager != null)
            {
                guideUIManager.NotifyArrivedNearPatient();
            }
        }
    }
}