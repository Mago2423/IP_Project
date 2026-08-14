using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class OfficerRInterrogation : MonoBehaviour
{
    [Header("Officer Movement")]
    [SerializeField] private NavMeshAgent officerAgent;
    [SerializeField] private Transform sidePosition;
    [SerializeField] private float sideStopDistance = 0.8f;

    [Header("Criminal Flow")]
    [SerializeField] private NpcNavPhaseController criminalPhaseController;
    [SerializeField] private bool startCriminalEnterImmediately = true;

    [Header("Runtime")]
    [SerializeField] private bool hasStartedSequence;

    private void Reset()
    {
        officerAgent = GetComponent<NavMeshAgent>();
    }

    private void Awake()
    {
        if (officerAgent == null)
        {
            officerAgent = GetComponent<NavMeshAgent>();
        }
    }

    public void BeginSequence()
    {
        if (hasStartedSequence)
        {
            return;
        }

        hasStartedSequence = true;
        MoveOfficerToSide();

        if (startCriminalEnterImmediately && criminalPhaseController != null)
        {
            criminalPhaseController.CallToTable();
        }
    }

    private void MoveOfficerToSide()
    {
        if (officerAgent == null || !officerAgent.enabled)
        {
            return;
        }

        if (sidePosition == null)
        {
            officerAgent.isStopped = true;
            if (officerAgent.hasPath)
            {
                officerAgent.ResetPath();
            }

            officerAgent.velocity = Vector3.zero;
            return;
        }

        officerAgent.stoppingDistance = sideStopDistance;
        officerAgent.isStopped = false;
        officerAgent.SetDestination(sidePosition.position);
    }
}