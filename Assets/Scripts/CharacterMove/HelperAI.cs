using UnityEngine;
using UnityEngine.AI;

public class HelperAI : MonoBehaviour
{
    public Transform target; // 쓰러진 캐릭터
    public float stopDistance = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isCalled = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isCalled || target == null) return;

        agent.SetDestination(target.position);

        // 도착 체크
        if (!agent.pathPending && agent.remainingDistance < stopDistance)
        {
            agent.isStopped = true;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("Call"); // 전화 애니메이션
            }

            isCalled = false;
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsMoving", true);
        }
    }

    public void CallToTarget(Transform fallenTarget)
    {
        target = fallenTarget;
        isCalled = true;
        agent.isStopped = false;
    }
}