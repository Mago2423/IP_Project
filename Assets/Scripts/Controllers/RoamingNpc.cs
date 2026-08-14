/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for RoamingNpc.
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
/// <summary>
/// Controls roaming NPC behavior and dialogue-triggered movement pauses.
/// </summary>
public class RoamingNpc : MonoBehaviour, IInteractable
{
    [Header("Movement")]
    [SerializeField, HideInInspector] private NavMeshAgent agent;
    [SerializeField, HideInInspector] private Rigidbody physicsBody;
    [SerializeField] private List<Transform> roamPoints = new();
    [SerializeField] private float stopDistance = 0.75f;
    [SerializeField] private float pauseDuration = 1.5f;

    [Header("Random Roaming")]
    [SerializeField] private bool useRandomRoaming = true;
    [SerializeField] private Transform randomRoamCenter;
    [SerializeField] private float randomRoamRadius = 4f;
    [SerializeField, HideInInspector] private float randomRoamSampleRange = 6f;
    [SerializeField, HideInInspector] private float playerAvoidRadius = 1.25f;
    [SerializeField, HideInInspector] private int randomRoamSampleAttempts = 8;

    [Header("Interaction")]
    [SerializeField, HideInInspector] private DialogueInteractable dialogueInteractable;

    private int _currentPointIndex = -1;
    private bool _isWaitingAtPoint;
    private float _waitTimer;
    private bool _isSpeaking;
    private Transform _playerTransform;
    private DialogueManager _dialogueManager;

/// <summary>
/// Resets the NPC movement state for the current scene.
/// </summary>
    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        physicsBody = GetComponent<Rigidbody>();
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

        if (physicsBody == null)
        {
            physicsBody = GetComponent<Rigidbody>();
        }

        ConfigurePhysicsBody();

        if (dialogueInteractable == null)
        {
            dialogueInteractable = GetComponent<DialogueInteractable>();
        }

        if (_playerTransform == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        if (_dialogueManager == null)
        {
            _dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
        }
    }

/// <summary>
/// Performs the configure physics body action.
/// </summary>
    private void ConfigurePhysicsBody()
    {
        if (physicsBody == null)
        {
            return;
        }

        // Prevent physics from nudging NavMesh-driven NPCs.
        physicsBody.isKinematic = true;
        physicsBody.useGravity = false;
        physicsBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        physicsBody.linearVelocity = Vector3.zero;
        physicsBody.angularVelocity = Vector3.zero;
    }

/// <summary>
/// Initializes gameplay state when the script begins running.
/// </summary>
    private void Start()
    {
        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updateUpAxis = false;
        }

        GoToNextRoamTarget();
    }

/// <summary>
/// Updates the movement state on the physics tick.
/// </summary>
    private void FixedUpdate()
    {
        if (agent == null)
        {
            return;
        }

        if (_isSpeaking)
        {
            StopAgentMovement(resetPath: true);

            if (_dialogueManager == null)
            {
                _dialogueManager = FindFirstObjectByType<DialogueManager>();
            }

            if (_dialogueManager == null || !_dialogueManager.IsDialogueActive)
            {
                ResumeRoamingAfterDialogue();
            }

            return;
        }

        if (roamPoints.Count == 0)
        {
            if (useRandomRoaming && !agent.pathPending && (!agent.hasPath || agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stopDistance)))
            {
                TrySetRandomDestination();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stopDistance))
        {
            if (!_isWaitingAtPoint)
            {
                _isWaitingAtPoint = true;
                _waitTimer = pauseDuration;
                StopAgentMovement(resetPath: true);
                return;
            }

            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaitingAtPoint = false;
                GoToNextRoamTarget();
            }
        }
    }

/// <summary>
/// Handles the interaction trigger for this object.
/// </summary>
    public void Interact()
    {
        BeginDialogueInteraction();

        if (dialogueInteractable != null)
        {
            dialogueInteractable.Interact();
        }
    }

/// <summary>
/// Starts dialogue mode and stops roaming while speaking.
/// </summary>
    public void BeginDialogueInteraction()
    {
        if (_isSpeaking)
        {
            return;
        }

        _isSpeaking = true;
        StopAgentMovement(resetPath: true);
    }

/// <summary>
/// Resumes roaming when the dialogue ends.
/// </summary>
    private void ResumeRoamingAfterDialogue()
    {
        _isSpeaking = false;
        _isWaitingAtPoint = false;
        _waitTimer = 0f;

        GoToNextRoamTarget();
    }

/// <summary>
/// Moves the NPC to the next valid roaming target.
/// </summary>
    private void GoToNextRoamTarget()
    {
        if (roamPoints.Count > 0)
        {
            for (int i = 0; i < roamPoints.Count; i++)
            {
                _currentPointIndex = (_currentPointIndex + 1) % roamPoints.Count;
                Transform target = roamPoints[_currentPointIndex];
                if (target == null || IsTargetBlocked(target.position))
                {
                    continue;
                }

                SetAgentDestination(target.position, stopDistance);
                return;
            }
        }

        if (useRandomRoaming)
        {
            TrySetRandomDestination();
        }
    }

/// <summary>
/// Attempts to move the NPC to a valid random roam point.
/// </summary>
    private void TrySetRandomDestination()
    {
        if (agent == null)
        {
            return;
        }

        Vector3 roamCenter = randomRoamCenter != null ? randomRoamCenter.position : transform.position;

        for (int attempt = 0; attempt < randomRoamSampleAttempts; attempt++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * randomRoamRadius;
            randomOffset.y = 0f;
            Vector3 desiredPoint = roamCenter + randomOffset;

            if (!NavMesh.SamplePosition(desiredPoint, out NavMeshHit navHit, randomRoamSampleRange, NavMesh.AllAreas))
            {
                continue;
            }

            if (IsTargetBlocked(navHit.position))
            {
                continue;
            }

            SetAgentDestination(navHit.position, stopDistance);
            return;
        }
    }

/// <summary>
/// Checks whether a target position is blocked by world geometry.
/// </summary>
    private bool IsTargetBlocked(Vector3 targetPosition)
    {
        if (_playerTransform == null)
        {
            return false;
        }

        return Vector3.SqrMagnitude(targetPosition - _playerTransform.position) <= playerAvoidRadius * playerAvoidRadius;
    }

/// <summary>
/// Sets the agent destination and ensures it is valid.
/// </summary>
    private void SetAgentDestination(Vector3 destination, float stoppingDistance)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (!NavMesh.SamplePosition(destination, out NavMeshHit navHit, 2f, agent.areaMask))
        {
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(navHit.position);
    }

/// <summary>
/// Stops current agent movement and optionally clears its path.
/// </summary>
    private void StopAgentMovement(bool resetPath)
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = true;
        agent.updateRotation = false;
        if (resetPath && agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }
}
