using UnityEngine;
using UnityEngine.AI;

public class GuideNPCAnimator : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Animator Param")]
    public string actionIdParam = "actionID";

    [Header("Action IDs")]
    public int idleId = 11;
    public int walkId = 21;

    [Header("Threshold")]
    public float moveSpeedThreshold = 0.1f;

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
    }
}