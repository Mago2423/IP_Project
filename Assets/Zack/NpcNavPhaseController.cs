using UnityEngine;
using UnityEngine.AI;

public class NpcNavPhaseController : MonoBehaviour
{
    public enum NpcPhase
    {
        Idle,
        Enter,
        Question,
        Leave,
        Finished
    }

    [Header("Scene References")]
    public NavMeshAgent agent;
    public Transform doorPoint;
    public Transform tablePoint;
    public Transform exitPoint;

    [Header("Phase Triggers (set from other scripts/UI later)")]
    public bool triggerEnterPhase;
    public bool triggerQuestionPhase;
    public bool triggerLeavePhase;

    [Header("Behavior")]
    public float stopDistance = 0.25f;

    [Header("Runtime State (read-only at runtime)")]
    [SerializeField] private NpcPhase currentPhase = NpcPhase.Idle;
    [SerializeField] private bool reachedDoor;
    [SerializeField] private bool reachedTable;

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
        HandleManualTriggers();

        switch (currentPhase)
        {
            case NpcPhase.Enter:
                UpdateEnterPhase();
                break;

            case NpcPhase.Question:
                UpdateQuestionPhase();
                break;

            case NpcPhase.Leave:
                UpdateLeavePhase();
                break;
        }
    }

    private void HandleManualTriggers()
    {
        if (triggerEnterPhase)
        {
            triggerEnterPhase = false;
            BeginEnterPhase();
        }

        if (triggerQuestionPhase)
        {
            triggerQuestionPhase = false;
            BeginQuestionPhase();
        }

        if (triggerLeavePhase)
        {
            triggerLeavePhase = false;
            BeginLeavePhase();
        }
    }

    public void BeginEnterPhase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        currentPhase = NpcPhase.Enter;
        reachedDoor = false;
        reachedTable = false;
        agent.isStopped = false;

        if (doorPoint != null)
        {
            agent.SetDestination(doorPoint.position);
        }
        else if (tablePoint != null)
        {
            reachedDoor = true;
            agent.SetDestination(tablePoint.position);
        }
    }

    public void BeginQuestionPhase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        currentPhase = NpcPhase.Question;

        if (tablePoint != null)
        {
            agent.SetDestination(tablePoint.position);
        }

        if (HasArrived())
        {
            agent.isStopped = true;
        }
    }

    public void BeginLeavePhase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        currentPhase = NpcPhase.Leave;
        agent.isStopped = false;

        if (exitPoint != null)
        {
            agent.SetDestination(exitPoint.position);
        }
    }

    private void UpdateEnterPhase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        if (!reachedDoor)
        {
            if (doorPoint == null)
            {
                reachedDoor = true;
            }
            else if (HasArrived())
            {
                reachedDoor = true;
            }

            if (reachedDoor && tablePoint != null)
            {
                agent.SetDestination(tablePoint.position);
            }

            return;
        }

        if (!reachedTable && tablePoint != null && HasArrived())
        {
            reachedTable = true;
            agent.isStopped = true;
        }
    }

    private void UpdateQuestionPhase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        if (!HasArrived())
        {
            return;
        }

        agent.isStopped = true;
    }

    private void UpdateLeavePhase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        if (exitPoint != null && HasArrived())
        {
            currentPhase = NpcPhase.Finished;
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
