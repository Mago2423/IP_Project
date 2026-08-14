using UnityEngine;
using UnityEngine.AI;

public class NpcNavPhaseController : MonoBehaviour
{
    public enum NpcPhase
    {
        Idle,
        MovingToTable,
        AtTable
    }

    [Header("Scene References")]
    public NavMeshAgent agent;
    public Transform tablePoint;

    [Header("Call Trigger")]
    public bool triggerCall;

    [Header("Behavior")]
    public float stopDistance = 0.25f;

    [Header("Runtime State (read-only at runtime)")]
    [SerializeField] private NpcPhase currentPhase = NpcPhase.Idle;

    public NpcPhase CurrentPhase => currentPhase;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (triggerCall)
        {
            triggerCall = false;
            CallToTable();
        }

        if (currentPhase == NpcPhase.MovingToTable)
        {
            UpdateMovementToTable();
        }
    }

    public void CallToTable()
    {
        if (!CanUseAgent())
        {
            return;
        }

        currentPhase = NpcPhase.MovingToTable;
        agent.isStopped = false;

        if (tablePoint != null)
        {
            agent.SetDestination(tablePoint.position);
        }
    }

    private void UpdateMovementToTable()
    {
        if (!CanUseAgent())
        {
            return;
        }

        if (tablePoint != null && HasArrived())
        {
            currentPhase = NpcPhase.AtTable;
            agent.isStopped = true;
        }
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled;
    }

    private bool HasArrived()
    {
        if (agent.pathPending)
        {
            return false;
        }

        return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stopDistance);
    }
}
