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

    [Header("Ray Visual")]
    public LineRenderer lineRenderer;
    public float lineVisibleTime = 0.1f;

    private float lineTimer = 0f;

    private void Update()
    {
        // Ray 궤적 표시 시간 감소
        if (lineRenderer != null && lineRenderer.enabled)
        {
            lineTimer -= Time.deltaTime;

            if (lineTimer <= 0f)
            {
                lineRenderer.enabled = false;
            }
        }

        // 현재 손의 트리거 입력만 받음
        if (IsThisHandTriggerDown())
        {
            FireRayFromController();
        }
    }

    private bool IsThisHandTriggerDown()
    {
        if (handType == HandType.Left)
        {
            return OVRInput.GetDown(
                OVRInput.Button.PrimaryIndexTrigger,
                OVRInput.Controller.LTouch
            );
        }

        return OVRInput.GetDown(
            OVRInput.Button.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );
    }

    private void FireRayFromController()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;

        Vector3 endPoint = ray.origin + ray.direction * rayDistance;

        // Ray가 무언가에 맞으면 맞은 위치까지만 선 표시
        if (Physics.Raycast(ray, out hit, rayDistance, interactLayer))
        {
            endPoint = hit.point;
        }

        ShowRayLine(ray.origin, endPoint);

        CheckRayHit(ray);
    }

    private void ShowRayLine(Vector3 startPoint, Vector3 endPoint)
    {
        if (lineRenderer == null)
        {
            Debug.LogWarning($"{handType} LineRenderer가 연결되지 않음");
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        lineTimer = lineVisibleTime;
    }

    private void CheckRayHit(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, interactLayer);

        if (hits.Length == 0)
        {
            Debug.Log($"{handType} Ray Hit 없음");
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            ClickableTarget clickableTarget =
                hit.collider.GetComponentInParent<ClickableTarget>();

            if (clickableTarget != null)
            {
                Debug.Log($"{handType} ClickableTarget 감지: {hit.collider.name}");
                clickableTarget.Interact();
                return;
            }

            OrangeShockButton shockButton =
                hit.collider.GetComponentInParent<OrangeShockButton>();

            if (shockButton != null)
            {
                Debug.Log($"{handType} ShockButton 감지: {hit.collider.name}");
                shockButton.Interact();
                return;
            }
        }

        Debug.Log($"{handType} Ray는 맞았지만 상호작용 대상 없음");
    }
}