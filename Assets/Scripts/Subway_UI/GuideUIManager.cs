using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GuideStep
{
    Introduction,
    ControlGuide,      // 추가
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

    [Header("엔딩 / 구조대원")]
    public GameObject[] rescuers;

    public Transform[] rescuerTargets;

    public float rescuerMoveSpeed = 2f;
    public float rescuerArriveDistance = 0.2f;
    public string menuSceneName = "MenuScene";

    [Header("구조대원 애니메이션")]
    public Animator[] rescuerAnimators;

    private bool rescuersMoving = false;
    private bool[] rescuerArrived;

    private GuideStep currentStep;
    private bool womanMovingToCPR = false;

    void Start()
    {
        currentStep = GuideStep.Introduction;
        ShowCurrentGuide();

        suitBody.SetActive(true);
        noneBody.SetActive(false);

        rescuerArrived = new bool[rescuers.Length];

        // 시작할 때 구조대원을 숨기고 싶으면 사용
        for (int i = 0; i < rescuers.Length; i++)
        {
            if (rescuers[i] != null)
            {
                rescuers[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        MoveWomanToCPR();

        if (rescuersMoving)
        {
            MoveRescuersToTargets();
        }
    }

    private void MoveWomanToCPR()
    {
        if (!womanMovingToCPR || woman == null || cprPosition == null)
            return;

        if (womanCollider != null)
        {
            womanCollider.enabled = false;
        }

        if (womanAgent != null)
        {
            if (womanAgent.enabled && womanAgent.isOnNavMesh)
            {
                womanAgent.isStopped = true;
                womanAgent.ResetPath();
            }

            womanAgent.enabled = false;
        }

        woman.position = Vector3.MoveTowards(
            woman.position,
            cprPosition.position,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction = cprPosition.position - woman.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            woman.rotation = Quaternion.Slerp(
                woman.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5f
            );
        }

        if (Vector3.Distance(woman.position, cprPosition.position) < 0.1f)
        {
            Debug.Log("여자 CPR 위치 도착");

            womanMovingToCPR = false;

            if (womanAnimator != null)
            {
                Debug.Log("CPR Trigger 실행");
                womanAnimator.SetTrigger("CPR");
            }

            currentStep = GuideStep.OpenAED;
            ShowCurrentGuide();
        }
    }

    private void MoveRescuersToTargets()
    {
        bool allArrived = true;

        for (int i = 0; i < rescuers.Length; i++)
        {
            if (i >= rescuerTargets.Length)
                continue;

            GameObject rescuer = rescuers[i];
            Transform target = rescuerTargets[i];

            if (rescuer == null || target == null)
                continue;

            // 이미 도착한 구조대원은 더 이상 이동하지 않음
            if (rescuerArrived[i])
                continue;

            float distance = Vector3.Distance(rescuer.transform.position, target.position);

            if (distance <= rescuerArriveDistance)
            {
                rescuerArrived[i] = true;

                if (i < rescuerAnimators.Length && rescuerAnimators[i] != null)
                {
                    rescuerAnimators[i].SetBool("IsMoving", false);
                }

                // 도착 후 서로 밀리지 않게 콜라이더 비활성화
                Collider col = rescuer.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                Debug.Log("구조대원 " + i + " 도착");
                continue;
            }

            allArrived = false;

            rescuer.transform.position = Vector3.MoveTowards(
                rescuer.transform.position,
                target.position,
                rescuerMoveSpeed * Time.deltaTime
            );

            Vector3 direction = target.position - rescuer.transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                rescuer.transform.rotation = Quaternion.Slerp(
                    rescuer.transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 5f
                );
            }
        }

        for (int i = 0; i < rescuerArrived.Length; i++)
        {
            if (!rescuerArrived[i])
            {
                allArrived = false;
                break;
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

        for (int i = 0; i < rescuers.Length; i++)
        {
            rescuerArrived[i] = false;

            if (rescuers[i] != null)
            {
                rescuers[i].SetActive(true);
            }

            if (i < rescuerAnimators.Length && rescuerAnimators[i] != null)
            {
                rescuerAnimators[i].SetBool("IsMoving", true);
            }
        }

        rescuersMoving = true;
    }

    private void ShowCurrentGuide()
    {
        switch (currentStep)
        {
            case GuideStep.Introduction:
                guideText.text = "사람이 쓰러졌을 때 대처 방법을 체험하는 프로그램입니다.";
                break;

            case GuideStep.ControlGuide:
                guideText.text =
                    "조작 방법 안내\n\n" +
                    "왼손 스틱으로 이동할 수 있습니다.\n" +
                    "컨트롤러 트리거를 눌러 Ray를 발사하여 물체를 선택할 수 있습니다.\n" +
                    "B 버튼을 눌러 현재 안내를 다시 확인할 수 있습니다.";
                break;

            case GuideStep.GoToPatientWithAED:
                guideText.text = "사람이 쓰러졌습니다. AED를 가지고 쓰러진 사람에게 다가가시오.";
                break;

            case GuideStep.SelectCarpenter:
                guideText.text = "목수복을 입은 사람을 클릭하여 119에 전화하게 하시오.";
                break;

            case GuideStep.RemovePatientClothes:
                guideText.text = "환자의 상체를 클릭하여 옷을 벗기시오.";
                break;

            case GuideStep.SelectWomanForCPR:
                guideText.text = "빨간색 옷을 입은 여자를 클릭하여 CPR을 실시하게 하시오.";
                break;

            case GuideStep.OpenAED:
                guideText.text = "AED의 아래에 있는 버튼을 클릭하여 열고, 음성 안내에 따라 진행하시오.";
                break;

            case GuideStep.RescueArrive:
                guideText.text = "구조대원이 환자에게 이동 중입니다.";
                break;

            case GuideStep.Ending:
                guideText.text = "응급 처치가 완료되었습니다. 구조대원에게 환자를 인계합니다.";
                break;

            case GuideStep.Finished:
                guideText.text = "안내가 완료되었습니다.";
                break;
        }
    }
}