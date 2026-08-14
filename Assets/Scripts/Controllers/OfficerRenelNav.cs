/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for OfficerJamalNav.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
/// <summary>
/// Controls the officer navigation path during interrogation events.
/// </summary>
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
    [SerializeField] private Transform interrogationDoorPoint;
    [SerializeField] private float patrolStopDistance = 0.75f;
    [SerializeField] private float doorStopDistance = 1.2f;
    [SerializeField] private float patrolPointPauseDuration = 1.5f;

    [Header("Interaction")]
    [SerializeField] private DialogueInteractable dialogueInteractable;

    [SerializeField] private OfficerState currentState = OfficerState.Patrolling;
    [SerializeField] private int patrolIndex = -1;
    [SerializeField] private bool hasStartedSequence;

    private bool _isWaitingAtPatrolPoint;
    private float _patrolPauseTimer;
    private Coroutine _dialogueSequenceCoroutine;

/// <summary>
/// Resets the NPC movement state for the current scene.
/// </summary>
    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (dialogueInteractable == null)
        {
            dialogueInteractable = GetComponent<DialogueInteractable>();
        }

        if (agent != null)
        {
            agent.stoppingDistance = patrolStopDistance;
        }
    }

/// <summary>
/// Initializes gameplay state when the script begins running.
/// </summary>
    private void Start()
    {
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        SetUprightRotation(transform.eulerAngles.y);

        if (patrolPoints.Count > 0)
        {
            MoveToNextPatrolPoint();
        }
        else
        {
            StopAgentImmediately();
        }
    }

/// <summary>
/// Updates the gameplay logic each frame.
/// </summary>
    private void Update()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        if (!hasStartedSequence && dialogueInteractable != null && dialogueInteractable.HasTriggered)
        {
            StartDoorSequenceAfterDialogue();
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

/// <summary>
/// Handles the interaction trigger for this object.
/// </summary>
    public void Interact()
    {
        if (hasStartedSequence || currentState == OfficerState.Speaking || currentState == OfficerState.MovingToDoor || currentState == OfficerState.WaitingAtDoor)
        {
            return;
        }

        StartDoorSequenceAfterDialogue();

        if (dialogueInteractable != null)
        {
            dialogueInteractable.Interact();
        }
    }

/// <summary>
/// Performs the start door sequence after dialogue action.
/// </summary>
    private void StartDoorSequenceAfterDialogue()
    {
        if (hasStartedSequence)
        {
            return;
        }

        hasStartedSequence = true;
        _isWaitingAtPatrolPoint = false;
        _patrolPauseTimer = 0f;
        currentState = OfficerState.Speaking;
        StopAgentImmediately();

        if (_dialogueSequenceCoroutine != null)
        {
            StopCoroutine(_dialogueSequenceCoroutine);
        }

        _dialogueSequenceCoroutine = StartCoroutine(WaitForDialogueThenMoveToDoor());
    }

/// <summary>
/// Performs the wait for dialogue then move to door action.
/// </summary>
    private IEnumerator WaitForDialogueThenMoveToDoor()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        while (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            yield return null;
        }

        _dialogueSequenceCoroutine = null;
        MoveToInterrogationDoor();
    }

/// <summary>
/// Performs the handle patrolling action.
/// </summary>
    private void HandlePatrolling()
    {
        if (patrolPoints.Count == 0)
        {
            StopAgentImmediately();
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

/// <summary>
/// Performs the handle moving to door action.
/// </summary>
    private void HandleMovingToDoor()
    {
        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, doorStopDistance))
        {
            StopAgentImmediately();
            currentState = OfficerState.WaitingAtDoor;
        }
    }

/// <summary>
/// Updates the camera position after the player has moved.
/// </summary>
    private void LateUpdate()
    {
        if (agent == null || !agent.enabled || agent.isStopped)
        {
            SetUprightRotation(transform.eulerAngles.y);
            return;
        }

        Vector3 movementDirection = agent.desiredVelocity;
        movementDirection.y = 0f;
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            float targetYaw = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;
            SetUprightRotation(targetYaw);
        }
    }

/// <summary>
/// Performs the set upright rotation action.
/// </summary>
    private void SetUprightRotation(float yaw)
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

/// <summary>
/// Performs the move to next patrol point action.
/// </summary>
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
            agent.updateRotation = false;
            agent.SetDestination(targetPoint.position);
            agent.isStopped = false;
        }
    }

/// <summary>
/// Performs the move to interrogation door action.
/// </summary>
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
            agent.updateRotation = false;
            agent.SetDestination(interrogationDoorPoint.position);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
            currentState = OfficerState.WaitingAtDoor;
        }
    }

/// <summary>
/// Performs the stop agent immediately action.
/// </summary>
    private void StopAgentImmediately()
    {
        agent.isStopped = true;
        agent.updateRotation = false;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }
}
