using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RoamingNpc : MonoBehaviour, IInteractable
{
    [Header("Movement")]
    [SerializeField, HideInInspector] private NavMeshAgent agent;
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

    private void Start()
    {
        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }

        GoToNextRoamTarget();
    }

    private void FixedUpdate()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        if (_isSpeaking)
        {
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

    public void Interact()
    {
        BeginDialogueInteraction();

        if (dialogueInteractable != null)
        {
            dialogueInteractable.Interact();
        }
    }

    public void BeginDialogueInteraction()
    {
        if (_isSpeaking)
        {
            return;
        }

        _isSpeaking = true;
        StopAgentMovement(resetPath: true);
    }

    private void ResumeRoamingAfterDialogue()
    {
        _isSpeaking = false;
        _isWaitingAtPoint = false;
        _waitTimer = 0f;

        GoToNextRoamTarget();
    }

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

    private bool IsTargetBlocked(Vector3 targetPosition)
    {
        if (_playerTransform == null)
        {
            return false;
        }

        return Vector3.SqrMagnitude(targetPosition - _playerTransform.position) <= playerAvoidRadius * playerAvoidRadius;
    }

    private void SetAgentDestination(Vector3 destination, float stoppingDistance)
    {
        if (agent == null)
        {
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    private void StopAgentMovement(bool resetPath)
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = true;
        if (resetPath && agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }
}
