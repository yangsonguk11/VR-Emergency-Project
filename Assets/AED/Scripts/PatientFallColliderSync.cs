using UnityEngine;

public class PatientColliderFallSync : MonoBehaviour
{
    public Animator animator;
    public CapsuleCollider bodyCollider;

    public string fallStateName = "FallDown";
    public float applyAfterNormalizedTime = 0.7f;

    public int fallenDirection = 2; // 0=X, 1=Y, 2=Z
    public Vector3 fallenCenter = new Vector3(0f, 0.25f, 0f);
    public float fallenHeight = 1.8f;
    public float fallenRadius = 0.18f;

    private bool applied = false;

    private void Update()
    {
        if (applied)
            return;

        if (animator == null || bodyCollider == null)
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName(fallStateName) && state.normalizedTime >= applyAfterNormalizedTime)
        {
            bodyCollider.direction = fallenDirection;
            bodyCollider.center = fallenCenter;
            bodyCollider.height = fallenHeight;
            bodyCollider.radius = fallenRadius;

            applied = true;

            Debug.Log("¾²·¯Áü °¨Áö ¡æ Capsule Collider ´¯Èû");
        }
    }
}