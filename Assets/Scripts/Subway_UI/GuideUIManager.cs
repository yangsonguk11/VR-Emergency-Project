using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GuideStep
{
    Introduction,
    GoToPatientWithAED,
    SelectCarpenter,
    RemovePatientClothes,
    SelectWomanForCPR,
    OpenAED,
    RescueArrive,
    Ending,
    Finished
}

public enum TargetType
{
    Carpenter,
    Patient,
    WomanRedClothes,
    AED
}

public class GuideUIManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI guideText;

    [Header("애니메이션")]
    public Animator carpenterAnimator;
    public Animator womanAnimator;

    [Header("환자 옷")]
    public GameObject suitBody;
    public GameObject noneBody;
   
    private bool changed = false;

    [Header("CPR 이동 위치")]
    public Transform woman;
    public Transform cprPosition;
    public float moveSpeed = 2f;

    public Collider womanCollider;
    public NavMeshAgent womanAgent;
    public MonoBehaviour womanAIScript; 
    public Collider patientCollider;
    public NavMeshAgent patientAgent;

    [Header("엔딩")]
    public GameObject[] rescuers;
    public Transform rescueTarget;
    public float rescuerMoveSpeed = 2f;
    public string menuSceneName = "MenuScene";

    private bool rescuersMoving = false;


    private GuideStep currentStep;

    private bool womanMovingToCPR = false;

    void Start()
    {
        currentStep = GuideStep.Introduction;
        ShowCurrentGuide();
        suitBody.SetActive(true);
        noneBody.SetActive(false);
    }

    void Update()
    {
        // 빨간 옷 여자가 환자에게 이동하는 처리
        if (womanMovingToCPR && woman != null && cprPosition != null)
        {

            // 여자 콜라이더를 아예 꺼도 됨
            if (womanCollider != null)
            {
                womanCollider.enabled = false;
            }

            if (womanAgent != null)
            {
                womanAgent.isStopped = true;
                womanAgent.ResetPath();
                womanAgent.enabled = false;
            }

            woman.position = Vector3.MoveTowards(
                woman.position,
                cprPosition.position,
                moveSpeed * Time.deltaTime
            );

            // 도착하면 CPR 애니메이션 실행
            if (Vector3.Distance(woman.position, cprPosition.position) < 0.1f)
            {
                Debug.Log("여자 CPR 위치 도착");

                womanMovingToCPR = false;

                if (womanAnimator != null)
                {
                    Debug.Log("CPR Trigger 실행");
                    womanAnimator.SetTrigger("CPR");
                }
                else
                {
                    Debug.LogError("womanAnimator가 연결 안 됨");
                }

                currentStep = GuideStep.OpenAED;
                ShowCurrentGuide();
            }
        }

        // 구조대원들이 환자 쪽으로 이동하는 처리
        if (rescuersMoving)
        {
            MoveRescuersToPatient();
        }
    }

    private void MoveRescuersToPatient()
    {
        bool allArrived = true;

        foreach (GameObject rescuer in rescuers)
        {
            if (rescuer == null)
            {
                continue;
            }

            rescuer.transform.position = Vector3.MoveTowards(
                rescuer.transform.position,
                rescueTarget.position,
                rescuerMoveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(rescuer.transform.position, rescueTarget.position) > 0.2f)
            {
                allArrived = false;
            }
        }

        if (allArrived)
        {
            rescuersMoving = false;
            StartCoroutine(EndingRoutine());
        }
    }

    private IEnumerator EndingRoutine()
    {
        currentStep = GuideStep.Ending;
        ShowCurrentGuide();

        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(menuSceneName);
    }

    public void OnTargetClicked(TargetType targetType, GameObject clickedObject)
    {
        switch (currentStep)
        {
            case GuideStep.SelectCarpenter:
                if (targetType == TargetType.Carpenter)
                {
                    if (carpenterAnimator != null)
                    {
                        carpenterAnimator.SetTrigger("Call");
                    }

                    currentStep = GuideStep.RemovePatientClothes;
                    ShowCurrentGuide();
                }
                break;

            case GuideStep.RemovePatientClothes:
                if (targetType == TargetType.Patient)
                {
                    YesChangeClothes();

                    currentStep = GuideStep.SelectWomanForCPR;
                    ShowCurrentGuide();
                }
                break;

            case GuideStep.SelectWomanForCPR:
                if (targetType == TargetType.WomanRedClothes)
                {
                    womanMovingToCPR = true;
                    guideText.text = "빨간색 옷을 입은 여자가 환자에게 이동 중입니다.";
                }
                break;

            case GuideStep.OpenAED:
                if (targetType == TargetType.AED)
                {
                    guideText.text = "AED를 열고 음성 안내에 따라 진행하시오.";

                    currentStep = GuideStep.Finished;
                }
                break;

            case GuideStep.RescueArrive:
                guideText.text = "구조대원이 도착했습니다.";
                break;

            case GuideStep.Ending:
                guideText.text = "응급 처치가 완료되었습니다. 구조대원에게 환자를 인계합니다.";
                break;
        }
    }

    public void YesChangeClothes()
    {
        suitBody.SetActive(false);
        noneBody.SetActive(true);

        changed = true;
    }

    public void NoClose()
    {
        changed = true;
    }

    public void OnPatientCollapsed()
    {
        if (currentStep == GuideStep.Introduction)
        {
            currentStep = GuideStep.GoToPatientWithAED;
            ShowCurrentGuide();
        }
    }

    public void NotifyArrivedNearPatient()
    {
        // 플레이어가 환자 근처에 도착했을 때 호출
        if (currentStep == GuideStep.GoToPatientWithAED)
        {
            currentStep = GuideStep.SelectCarpenter;
            ShowCurrentGuide();
        }
    }

    public void OnAEDCompleted()
    {
        Debug.Log("AED 완료됨. 구조대원 단계 시작");
        currentStep = GuideStep.RescueArrive;
        ShowCurrentGuide();

        // 구조대원 활성화
        foreach (GameObject rescuer in rescuers)
        {
            if (rescuer != null)
            {
                rescuer.SetActive(true);
            }
        }

        rescuersMoving = true;
    }



    private void ShowCurrentGuide()
    {
        switch (currentStep)
        {
            case GuideStep.Introduction:
                guideText.text = "사람이 쓰러졌을떄 대처 방법을 체험하는 프로그램입니다.";
                break;

            case GuideStep.GoToPatientWithAED:
                guideText.text = "사람이 쓰러졌습니다. AED를 가지고 쓰러진 사람에게 다가가시오.";
                break;

            case GuideStep.SelectCarpenter:
                guideText.text = "목수복을 입은 사람을 지목하여 119에 전화하게 하시오.";
                break;

            case GuideStep.RemovePatientClothes:
                guideText.text = "환자의 옷을 벗기시오.";
                break;

            case GuideStep.SelectWomanForCPR:
                guideText.text = "빨간색 옷을 입은 여자를 지목하여 CPR을 실시하게 하시오.";
                break;

            case GuideStep.OpenAED:
                guideText.text = "AED를 열어 나오는 음성에 따라 진행하시오.";
                break;

            case GuideStep.Finished:
                guideText.text = "안내가 완료되었습니다.";
                break;
        }
    }
}