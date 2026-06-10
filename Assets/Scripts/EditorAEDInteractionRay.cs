using UnityEngine;

public class EditorAEDInteractionRay : MonoBehaviour
{
    public Camera playerCamera;

    public float clickDistance = 5f;
    public float dragDistance = 2f;

    public LayerMask buttonLayer;
    public LayerMask padLayer;
    public LayerMask bodyLayer;

    public AEDLidOpener aedLidOpener;

    private Transform draggingObject;
    private Vector3 offset;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // AED 뚜껑 열기 버튼 클릭
            if (TryClickButton())
                return;

            // 뚜껑이 열린 후에만 패드 드래그 가능
            if (aedLidOpener != null && aedLidOpener.IsOpened())
            {
                if (TryStartDrag(padLayer))
                    return;
            }

            // 환자 몸 드래그
            TryStartDrag(bodyLayer);
        }

        if (Input.GetMouseButton(0) && draggingObject != null)
        {
            DragObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingObject = null;
        }
    }

    bool TryClickButton()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, clickDistance, buttonLayer, QueryTriggerInteraction.Collide))
        {
            AEDLidPressButton button = hit.collider.GetComponentInParent<AEDLidPressButton>();

            if (button != null)
            {
                button.PressButton();
                return true;
            }
        }

        return false;
    }

    bool TryStartDrag(LayerMask layer)
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, clickDistance, layer, QueryTriggerInteraction.Collide))
        {
            AEDPadAttach padAttach = hit.collider.GetComponentInParent<AEDPadAttach>();

            if (padAttach != null)
            {
                // 이미 부착된 패드는 드래그 금지
                if (padAttach.IsAttached())
                {
                    Debug.Log("이미 부착된 패드라서 드래그 불가");
                    return false;
                }

                draggingObject = padAttach.transform;

                // 에디터에서도 Grab 처리
                padAttach.MarkGrabbedForEditor();
            }
            else
            {
                draggingObject = hit.collider.transform;
            }

            Rigidbody rb = draggingObject.GetComponent<Rigidbody>();

            if (rb != null)
                rb.isKinematic = true;

            Vector3 mouseWorld = GetMouseWorldPosition();
            offset = draggingObject.position - mouseWorld;

            return true;
        }

        return false;
    }

    void DragObject()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        draggingObject.position = mouseWorld + offset;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = dragDistance;

        return playerCamera.ScreenToWorldPoint(mousePos);
    }
}