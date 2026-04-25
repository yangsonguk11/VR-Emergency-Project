using UnityEngine;
using UnityEngine.AI;

public class PatrolNav : MonoBehaviour
{
    public Transform[] points;
    public float waitTime = 1f;

    private int currentIndex = 0;
    private bool isWaiting = false;

    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (points.Length > 0)
            agent.SetDestination(points[0].position);
    }

    void Update()
    {
        if (points.Length == 0 || isWaiting)
            return;

        // 목적지 도착 체크
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            StartCoroutine(WaitAndNext());
        }

        // 애니메이션
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);
        }
    }

    System.Collections.IEnumerator WaitAndNext()
    {
        isWaiting = true;

        if (animator != null)
            animator.SetBool("IsMoving", false);

        yield return new WaitForSeconds(waitTime);

        currentIndex++;
        if (currentIndex >= points.Length)
            currentIndex = 0;

        agent.SetDestination(points[currentIndex].position);

        isWaiting = false;
    }
}