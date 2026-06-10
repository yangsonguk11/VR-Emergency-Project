using UnityEngine;

public class VRRayClicker : MonoBehaviour
{
    public enum HandType
    {
        Left,
        Right
    }

    [Header("Hand Settings")]
    public HandType handType;

    [Header("Ray Settings")]
    public float rayDistance = 10f;
    public LayerMask interactLayer;

    [Header("Debug")]
    public bool showDebugRay = true;

    private void Update()
    {
        // 현재 스크립트가 붙은 손의 입력만 체크
        if (IsThisHandTriggerDown())
        {
            FireRayFromController();
        }
    }

    private bool IsThisHandTriggerDown()
    {
        // 왼손이면 왼쪽 컨트롤러 트리거만 확인
        if (handType == HandType.Left)
        {
            return OVRInput.GetDown(
                OVRInput.Button.PrimaryIndexTrigger,
                OVRInput.Controller.LTouch
            );
        }

        // 오른손이면 오른쪽 컨트롤러 트리거만 확인
        return OVRInput.GetDown(
            OVRInput.Button.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );
    }

    private void FireRayFromController()
    {
        // 이 스크립트가 붙어있는 손 위치와 방향에서 Ray 발사
        Ray ray = new Ray(transform.position, transform.forward);

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f);
        }

        CheckRayHit(ray);
    }

    private void CheckRayHit(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, interactLayer);

        if (hits.Length == 0)
        {
            Debug.Log($"{handType} Ray Hit 없음");
            return;
        }

        // 가장 가까운 오브젝트부터 검사
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            ClickableTarget clickableTarget = hit.collider.GetComponentInParent<ClickableTarget>();

            if (clickableTarget != null)
            {
                Debug.Log($"{handType} ClickableTarget 감지: {hit.collider.name}");
                clickableTarget.Interact();
                return;
            }

            OrangeShockButton shockButton = hit.collider.GetComponentInParent<OrangeShockButton>();

            if (shockButton != null)
            {
                Debug.Log($"{handType} ShockButton 감지: {hit.collider.name}");
                shockButton.Interact();
                return;
            }

            ScenarioCloseButton closeButton = hit.collider.GetComponentInParent<ScenarioCloseButton>();

            if (closeButton != null)
            {
                Debug.Log("Scenario UI X 버튼 감지: " + hit.collider.name);
                closeButton.Interact();
                return;
            }
        }

        Debug.Log($"{handType} Ray는 맞았지만 상호작용 대상 없음");
    }
}