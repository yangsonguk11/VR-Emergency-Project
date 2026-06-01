using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("일시정지 메뉴로 사용할 Canvas 게임 오브젝트")]
    public GameObject pauseMenuUI;
    
    [Tooltip("사용자의 시선을 기준을 잡기 위한 카메라 (OVRCameraRig의 CenterEyeAnchor)")]
    public Transform centerEyeCamera;
    
    [Tooltip("메뉴가 나타날 거리")]
    public float spawnDistance = 1.5f;

    void Start()
    {
        // 시작할 때는 메뉴를 숨겨둡니다.
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        // 왼쪽 컨트롤러의 메뉴 버튼(Start 버튼)을 눌렀을 때 감지
        if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.LTouch))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (pauseMenuUI == null) return;

        bool isActive = !pauseMenuUI.activeSelf;
        pauseMenuUI.SetActive(isActive);

        // 메뉴를 켤 때만 사용자 앞쪽으로 위치를 재조정합니다.
        if (isActive)
        {
            PositionMenuInFrontOfUser();
        }
    }

    private void PositionMenuInFrontOfUser()
    {
        if (centerEyeCamera == null) return;

        // 카메라가 바라보는 앞쪽 방향(수평)을 계산합니다.
        Vector3 forwardDirection = centerEyeCamera.forward;
        forwardDirection.y = 0; // 메뉴가 위아래로 기울지 않고 수평을 유지하도록 y축 무시
        forwardDirection.Normalize();

        // 카메라 위치에서 앞쪽으로 spawnDistance 만큼 떨어진 곳에 배치
        Vector3 spawnPosition = centerEyeCamera.position + (forwardDirection * spawnDistance);
        
        // 메뉴의 높이를 사용자 눈높이보다 살짝 아래로 조정 (선택 사항)
        spawnPosition.y = centerEyeCamera.position.y - 0.2f;

        pauseMenuUI.transform.position = spawnPosition;

        // 메뉴가 사용자를 바라보도록 회전 설정
        pauseMenuUI.transform.forward = forwardDirection;
    }
}