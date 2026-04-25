using UnityEngine;
using UnityEngine.AI;

public class FallDownCharacter : MonoBehaviour
{
    public float fallDelay = 10f;

    private Animator animator;
    private PatrolNav patrolNav;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        patrolNav = GetComponent<PatrolNav>();
        agent = GetComponent<NavMeshAgent>();

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

        // 쓰러지는 애니메이션 실행
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("FallDown");
        }
    }
}