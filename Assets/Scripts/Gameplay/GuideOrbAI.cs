using UnityEngine;
using UnityEngine.AI;

public class GuideOrbAI : MonoBehaviour
{
    public enum State
    {
        FollowPlayer,
        LeadToTarget,
        WaitAtTarget,
        PromptInteract
    }

    [Header("Refs")]
    public Transform player;
    public Transform targetPoint;
    public NavMeshAgent agent;

    [Header("Follow (FollowPlayer)")]
    public float followDistance = 1.6f;
    public float followHeight = 1.6f;

    [Header("Lead / Arrive")]
    public float arriveDistance = 1.0f;

    [Header("State")]
    public State state = State.FollowPlayer;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        switch (state)
        {
            case State.FollowPlayer:
                TickFollowPlayer();
                break;

            case State.LeadToTarget:
                TickLeadToTarget();
                break;

            case State.WaitAtTarget:
                TickWaitAtTarget();
                break;

            case State.PromptInteract:
                TickPromptInteract();
                break;
        }
    }

    void TickFollowPlayer()
    {
        Vector3 desired = player.position + player.forward * followDistance;
        desired.y = player.position.y;

        agent.isStopped = false;
        agent.SetDestination(desired);
    }

    void TickLeadToTarget()
    {
        if (targetPoint == null)
        {
            state = State.FollowPlayer;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(targetPoint.position);

        if (agent.pathPending) return;

        if (agent.remainingDistance <= arriveDistance)
        {
            agent.isStopped = true;
            state = State.WaitAtTarget;
        }
    }

    void TickWaitAtTarget()
    {
        agent.isStopped = true;
    }

    void TickPromptInteract()
    {
        agent.isStopped = true;
    }

    public void LeadTo(Transform newTarget)
    {
        targetPoint = newTarget;
        state = State.LeadToTarget;
        agent.isStopped = false;
    }

    public void BackToPlayer()
    {
        targetPoint = null;
        state = State.FollowPlayer;
        agent.isStopped = false;
    }
}