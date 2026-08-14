/// <summary>
/// Author: Lee wei jun
/// StudentNo: 10272279E
/// Purpose:
/// The Script is responsible for triggering the interrogation sequence between the officer and the criminal NPC. It manages the officer's movement to a designated side position and coordinates the criminal's behavior through the NpcNavPhaseController.
/// </summary>

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Orchestrates an interrogation sequence between an officer and a criminal NPC.
/// Manages the officer's movement and controls the criminal's behavior through NpcNavPhaseController.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class OfficerRInterrogation : MonoBehaviour
{
    [Header("Officer Movement")]
    /// <summary>The NavMeshAgent component controlling the officer's movement.</summary>
    [SerializeField] private NavMeshAgent officerAgent;
    
    /// <summary>The target position transform where the officer should move to during the sequence.</summary>
    [SerializeField] private Transform sidePosition;
    
    /// <summary>The stopping distance for the officer when arriving at the side position.</summary>
    [SerializeField] private float sideStopDistance = 0.8f;

    [Header("Criminal Flow")]
    /// <summary>Reference to the criminal NPC's phase controller to coordinate their movement.</summary>
    [SerializeField] private NpcNavPhaseController criminalPhaseController;
    
    /// <summary>If true, the criminal will immediately begin moving to the table when the sequence starts.</summary>
    [SerializeField] private bool startCriminalEnterImmediately = true;

    [Header("Runtime")]
    /// <summary>Tracks whether the interrogation sequence has already been initiated.</summary>
    [SerializeField] private bool hasStartedSequence;

    /// <summary>
    /// Called when the component is reset in the editor.
    /// Automatically assigns the NavMeshAgent component if not already assigned.
    /// </summary>
    private void Reset()
    {
        officerAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Called on object initialization.
    /// Ensures the NavMeshAgent component is properly assigned before gameplay begins.
    /// </summary>
    private void Awake()
    {
        if (officerAgent == null)
        {
            officerAgent = GetComponent<NavMeshAgent>();
        }
    }

    /// <summary>
    /// Initiates the interrogation sequence.
    /// Moves the officer to the side position and triggers the criminal to move to the table.
    /// This method can only be called once; subsequent calls are ignored.
    /// </summary>
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

    /// <summary>
    /// Moves the officer to the designated side position.
    /// If no valid side position is assigned, stops the officer in place.
    /// Validates that the NavMeshAgent is available before executing movement commands.
    /// </summary>
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
