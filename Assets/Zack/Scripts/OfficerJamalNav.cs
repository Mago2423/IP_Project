using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class OfficerJamalNav : MonoBehaviour, IInteractable
{
    private enum OfficerState
    {
        Patrolling,
        Speaking,
        MovingToDoor,
        WaitingAtDoor
    }

    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField] private bool useRandomPatrolIfNoPoints = true;
    [SerializeField] private float randomPatrolRadius = 4f;
    [SerializeField] private float randomPatrolSampleRange = 6f;
    [SerializeField] private Transform interrogationDoorPoint;
    [SerializeField] private float patrolStopDistance = 0.75f;
    [SerializeField] private float doorStopDistance = 1.2f;
    [SerializeField] private float patrolPointPauseDuration = 1.5f;

    [Header("Interaction")]
    [SerializeField] private DialogueTrigger dialogueTrigger;

    [SerializeField] private OfficerState currentState = OfficerState.Patrolling;
    [SerializeField] private int patrolIndex = -1;
    [SerializeField] private bool hasStartedSequence;

    private bool _isWaitingAtPatrolPoint;
    private float _patrolPauseTimer;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.stoppingDistance = patrolStopDistance;
        }
    }

    private void Start()
    {
        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }

        if (patrolPoints.Count > 0)
        {
            MoveToNextPatrolPoint();
        }
        else if (useRandomPatrolIfNoPoints)
        {
            TrySetRandomPatrolDestination();
        }
    }

    private void Update()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        if (currentState == OfficerState.Speaking)
        {
            StopAgentImmediately();
            return;
        }

        switch (currentState)
        {
            case OfficerState.Patrolling:
                HandlePatrolling();
                break;

            case OfficerState.MovingToDoor:
                HandleMovingToDoor();
                break;
        }
    }

    public void Interact()
    {
        if (hasStartedSequence || currentState == OfficerState.Speaking || currentState == OfficerState.MovingToDoor || currentState == OfficerState.WaitingAtDoor)
        {
            return;
        }

        hasStartedSequence = true;
        _isWaitingAtPatrolPoint = false;
        _patrolPauseTimer = 0f;
        currentState = OfficerState.Speaking;
        StopAgentImmediately();

        if (dialogueTrigger != null)
        {
            dialogueTrigger.Interact();
        }
        else
        {
            Debug.LogWarning($"{nameof(OfficerJamalNav)} on {name} is missing a DialogueTrigger reference.", this);
        }

        StartCoroutine(WaitForDialogueThenMoveToDoor());
    }

    private IEnumerator WaitForDialogueThenMoveToDoor()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        while (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            yield return null;
        }

        MoveToInterrogationDoor();
    }

    private void HandlePatrolling()
    {
        if (patrolPoints.Count == 0)
        {
            if (useRandomPatrolIfNoPoints && !agent.pathPending && (!agent.hasPath || agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, patrolStopDistance)))
            {
                TrySetRandomPatrolDestination();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, patrolStopDistance))
        {
            if (!_isWaitingAtPatrolPoint)
            {
                _isWaitingAtPatrolPoint = true;
                _patrolPauseTimer = patrolPointPauseDuration;
                agent.isStopped = true;
                return;
            }

            _patrolPauseTimer -= Time.deltaTime;
            if (_patrolPauseTimer <= 0f)
            {
                _isWaitingAtPatrolPoint = false;
                MoveToNextPatrolPoint();
            }
        }
    }

    private void HandleMovingToDoor()
    {
        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, doorStopDistance))
        {
            StopAgentImmediately();
            currentState = OfficerState.WaitingAtDoor;
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Count == 0)
        {
            return;
        }

        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        Transform targetPoint = patrolPoints[patrolIndex];

        if (targetPoint != null)
        {
            agent.stoppingDistance = patrolStopDistance;
            agent.SetDestination(targetPoint.position);
            agent.isStopped = false;
        }
    }

    private void MoveToInterrogationDoor()
    {
        if (agent == null)
        {
            return;
        }

        currentState = OfficerState.MovingToDoor;

        if (interrogationDoorPoint != null)
        {
            agent.stoppingDistance = doorStopDistance;
            agent.SetDestination(interrogationDoorPoint.position);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
            currentState = OfficerState.WaitingAtDoor;
            Debug.LogWarning($"{nameof(OfficerJamalNav)} on {name} is missing an interrogation door target.", this);
        }
    }

    private void TrySetRandomPatrolDestination()
    {
        Vector3 randomOffset = Random.insideUnitSphere * randomPatrolRadius;
        randomOffset.y = 0f;
        Vector3 desiredPoint = transform.position + randomOffset;

        if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit navHit, randomPatrolSampleRange, NavMesh.AllAreas))
        {
            agent.stoppingDistance = patrolStopDistance;
            agent.isStopped = false;
            agent.SetDestination(navHit.position);
        }
    }

    private void StopAgentImmediately()
    {
        agent.isStopped = true;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }
}
