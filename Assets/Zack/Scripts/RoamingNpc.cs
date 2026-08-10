using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RoamingNpc : MonoBehaviour, IInteractable
{
    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private List<Transform> roamPoints = new();
    [SerializeField] private float stopDistance = 0.75f;
    [SerializeField] private float pauseDuration = 1.5f;
    [SerializeField] private bool useRandomRoaming = true;
    [SerializeField] private float randomRoamRadius = 4f;
    [SerializeField] private float randomRoamSampleRange = 6f;

    [Header("Interaction")]
    [SerializeField] private DialogueInteractable dialogueInteractable;

    private int _currentPointIndex = -1;
    private bool _isWaitingAtPoint;
    private float _waitTimer;
    private bool _isSpeaking;

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

    private void Update()
    {
        if (agent == null || !agent.enabled || _isSpeaking)
        {
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
                agent.isStopped = true;
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
        if (_isSpeaking)
        {
            return;
        }

        _isSpeaking = true;
        StopAgentImmediately();

        if (dialogueInteractable != null)
        {
            dialogueInteractable.Interact();
        }
        else
        {
            Debug.LogWarning($"{nameof(RoamingNpc)} on {name} is missing a DialogueInteractable reference.", this);
        }
    }

    private void GoToNextRoamTarget()
    {
        if (roamPoints.Count > 0)
        {
            _currentPointIndex = (_currentPointIndex + 1) % roamPoints.Count;
            Transform target = roamPoints[_currentPointIndex];
            if (target != null)
            {
                agent.stoppingDistance = stopDistance;
                agent.SetDestination(target.position);
                agent.isStopped = false;
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

        Vector3 randomOffset = Random.insideUnitSphere * randomRoamRadius;
        randomOffset.y = 0f;
        Vector3 desiredPoint = transform.position + randomOffset;

        if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit navHit, randomRoamSampleRange, NavMesh.AllAreas))
        {
            agent.stoppingDistance = stopDistance;
            agent.isStopped = false;
            agent.SetDestination(navHit.position);
        }
    }

    private void StopAgentImmediately()
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = true;
        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }
}
