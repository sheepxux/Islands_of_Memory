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
        if (!agent.enabled || !agent.isOnNavMesh) return;

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
        TrySetDestination(desired);
    }

    void TickLeadToTarget()
    {
        if (targetPoint == null)
        {
            state = State.FollowPlayer;
            return;
        }

        agent.isStopped = false;
        TrySetDestination(targetPoint.position);

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
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent == null) return;

        targetPoint = newTarget;
        state = State.LeadToTarget;
        if (agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public void BackToPlayer()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        targetPoint = null;
        state = State.FollowPlayer;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    private bool TrySetDestination(Vector3 destination)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, agent.areaMask))
        {
            agent.SetDestination(hit.position);
            return true;
        }

        return false;
    }
}
