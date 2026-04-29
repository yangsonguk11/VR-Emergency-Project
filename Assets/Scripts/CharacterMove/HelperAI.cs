using UnityEngine;
using UnityEngine.AI;

public class HelperAI : MonoBehaviour
{
    public Transform target;
    public float stopDistance = 1.5f;

    public bool willCall = false; // true인 캐릭터만 전화 애니메이션

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

        if (!agent.pathPending && agent.remainingDistance < stopDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);

                if (willCall)
                    animator.SetTrigger("Call");
                else
                    animator.SetTrigger("IdleReact");
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