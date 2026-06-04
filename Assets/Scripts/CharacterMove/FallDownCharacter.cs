using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FallDownCharacter : MonoBehaviour
{
    public float fallDelay = 10f;

    private Animator animator;
    private PatrolNav patrolNav;
    private NavMeshAgent agent;

    public List<HelperAI> helpers; // 여러 명

    public AudioClip fallSound;
    private AudioSource audioSource;

    public GoldenTimeCountdown countdown;

    public GuideUIManager guideUIManager;

    void Start()
    {
        animator = GetComponent<Animator>();
        patrolNav = GetComponent<PatrolNav>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        Invoke(nameof(FallDown), fallDelay);
    }

    public void FallDown()
    {
        // 순찰 스크립트 끄기
        if (patrolNav != null)
            patrolNav.enabled = false;

        // NavMeshAgent 이동 정지
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // 효과음 재생
        if (audioSource != null && fallSound != null)
        {
            audioSource.PlayOneShot(fallSound);
        }


        // 쓰러지는 애니메이션 실행
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("FallDown");
        }

        guideUIManager.OnPatientCollapsed();

        //if (countdown != null)
        //    countdown.StartCountdown();

        // 여러 NPC 호출
        foreach (var helper in helpers)
        {
            if (helper != null)
                helper.CallToTarget(transform);
        }

        //ClothesChangeUI clothesUI = GetComponent<ClothesChangeUI>();

        //if (clothesUI != null)
        //{
        //    clothesUI.SetFallen(true);
        //    Debug.Log("UI 쓰러짐 상태 true로 변경됨");
        //}
        //else
        //{
        //    Debug.Log("ClothesChangeUI가 이 오브젝트에 없음");
        //}
    }
}