using UnityEngine;

public class EditorAEDInteractionRay : MonoBehaviour
{
    public Camera playerCamera;

    public float clickDistance = 5f;
    public float dragDistance = 2f;

    public LayerMask buttonLayer;
    public LayerMask padLayer;
    public LayerMask bodyLayer;
    public LayerMask shockButtonLayer;

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
            if (TryClickShockButton())
                return;

            if (TryClickButton())
                return;

            if (aedLidOpener != null && aedLidOpener.IsOpened())
            {
                if (TryStartDrag(padLayer))
                    return;
            }

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
                // 이미 부착된 패드면 드래그 금지
                if (padAttach.IsAttached())
                {
                    Debug.Log("이미 부착된 패드라서 드래그 불가");
                    return false;
                }

                // 패드 루트 드래그
                draggingObject = padAttach.transform;

                // 에디터 드래그도 Grab 처리
                padAttach.MarkGrabbedForEditor();
            }
            else
            {
                // 일반 오브젝트 드래그
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

    bool TryClickShockButton()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        Debug.Log("주황색 버튼 Ray 발사");

        if (Physics.Raycast(ray, out RaycastHit hit, clickDistance, shockButtonLayer, QueryTriggerInteraction.Collide))
        {
            Debug.Log("주황색 버튼 Collider 맞음: " + hit.collider.name);

            OrangeShockButton shockButton =
                hit.collider.GetComponentInParent<OrangeShockButton>();

            if (shockButton != null)
            {
                Debug.Log("OrangeShockButton 컴포넌트 찾음");

                shockButton.PressShockButton();

                return true;
            }
            else
            {
                Debug.Log("OrangeShockButton 컴포넌트 없음");
            }
        }
        else
        {
            Debug.Log("주황색 버튼 Raycast 실패");
        }

        return false;
    }

}