//* Author: Lee wei jun
//* Date: 14/6/2026
//* Description: This script manages an NPC's movement through three distinct phases using Unity's NavMesh system
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages NPC navigation through different phases: Idle, Moving to Table, and At Table.
/// Uses Unity's NavMesh system to handle pathfinding and movement control.
/// </summary>
/// <summary>
/// Controls the navigation behavior of an NPC through different phases.
/// </summary>
public class NpcNavPhaseController : MonoBehaviour
{
    /// <summary>
    /// Represents the current state of the NPC's navigation behavior.
    /// </summary>
    public enum NpcPhase
    {
        /// <summary>NPC is stationary and not moving.</summary>
        Idle,
        /// <summary>NPC is actively navigating toward the table destination.</summary>
        MovingToTable,
        /// <summary>NPC has reached the table and stopped.</summary>
        AtTable
    }

    [Header("Scene References")]
    /// <summary>The NavMeshAgent component responsible for pathfinding and movement.</summary>
    public NavMeshAgent agent;
    
    /// <summary>The target position transform where the NPC should navigate to.</summary>
    public Transform tablePoint;

    [Header("Call Trigger")]
    /// <summary>Set to true to trigger the NPC to move toward the table. Automatically reset to false after processing.</summary>
    public bool triggerCall;

    [Header("Behavior")]
    /// <summary>The distance threshold for considering the NPC as arrived at the destination.</summary>
    public float stopDistance = 0.25f;

    [Header("Runtime State (read-only at runtime)")]
    /// <summary>The current phase/state of the NPC's navigation behavior.</summary>
    [SerializeField] private NpcPhase currentPhase = NpcPhase.Idle;

    /// <summary>Gets the current phase of the NPC's navigation state.</summary>
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

    /// <summary>
    /// Initiates the NPC's movement toward the table destination.
    /// Validates the NavMeshAgent is available before starting movement.
    /// </summary>
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

    /// <summary>
    /// Updates the NPC's movement state each frame while in the MovingToTable phase.
    /// Checks if arrival conditions are met and transitions to the AtTable phase when complete.
    /// </summary>
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

    /// <summary>
    /// Checks if the NavMeshAgent is available and enabled for use.
    /// </summary>
    /// <returns>True if the agent exists and is enabled, false otherwise.</returns>
    private bool CanUseAgent()
    {
        return agent != null && agent.enabled;
    }

    /// <summary>
    /// Determines whether the NPC has arrived at the destination.
    /// Accounts for pending path calculations and uses the maximum of the agent's and script's stopping distances.
    /// </summary>
    /// <returns>True if the NPC has reached the destination, false otherwise.</returns>
    private bool HasArrived()
    {
        if (agent.pathPending)
        {
            return false;
        }

        return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stopDistance);
    }
}
