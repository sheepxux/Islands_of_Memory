using UnityEngine;
using UnityEngine.AI;

public class GuideNPCAnimator : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform visualRoot;

    [Header("Animator Param")]
    public string actionIdParam = "actionID";

    [Header("Action IDs")]
    public int idleId = 11;
    public int walkId = 21;

    [Header("Threshold")]
    public float moveSpeedThreshold = 0.1f;

    [Header("Facing")]
    public bool faceMovementDirection = true;
    public float turnSpeed = 10f;
    public float yawOffset = 0f;

    private void Update()
    {
        if (agent == null || animator == null) return;

        float speed = agent.velocity.magnitude;

        if (speed > moveSpeedThreshold)
        {
            animator.SetInteger(actionIdParam, walkId);
        }
        else
        {
            animator.SetInteger(actionIdParam, idleId);
        }

        RotateTowardMovement(speed);
    }

    private void RotateTowardMovement(float speed)
    {
        if (!faceMovementDirection || speed <= moveSpeedThreshold) return;

        Transform targetRoot = visualRoot != null ? visualRoot : animator.transform;
        Vector3 direction = agent.velocity;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up) *
                                    Quaternion.Euler(0f, yawOffset, 0f);

        targetRoot.rotation = Quaternion.Slerp(
            targetRoot.rotation,
            targetRotation,
            1f - Mathf.Exp(-turnSpeed * Time.deltaTime)
        );
    }
}
