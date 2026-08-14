/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for DialogueInteractable.
/// </summary>

using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents a world object that triggers dialogue interaction when activated.
/// </summary>
public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private string startNodeId = "";
    [SerializeField] private bool oneShot;

    private bool _hasTriggered;

    public event Action DialogueStarted;

/// <summary>
/// Performs the has triggered action.
/// </summary>
    public bool HasTriggered => _hasTriggered;

/// <summary>
/// Performs the on trigger enter action.
/// </summary>
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interact();
        }
    }
/// <summary>
/// Handles the interaction trigger for this object.
/// </summary>
    public void Interact()
    {
        if (oneShot && _hasTriggered)
        {
            return;
        }

        // Linear NPC dialogues in VirtualWorld may not have choice actions,
        // so trigger evidence collection directly when this object is configured for it.
        if (TryGetComponent(out CaseFlowInteractable caseFlowInteractable))
        {
            caseFlowInteractable.Interact();
        }

        StopMovementForDialogue();
        FacePlayer();

        DialogueData resolvedDialogue = ResolveDialogueData();
        if (resolvedDialogue == null)
        {
            return;
        }

        DialogueManager manager = FindFirstObjectByType<DialogueManager>();
        if (manager == null)
        {
            return;
        }

        manager.StartDialogue(resolvedDialogue, startNodeId, this);
        _hasTriggered = true;
        DialogueStarted?.Invoke();
    }

/// <summary>
/// Performs the face player action.
/// </summary>
    private void FacePlayer()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            return;
        }

        Transform npcTransform = transform;
        NavMeshAgent navMeshAgent = GetComponentInParent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
            npcTransform = navMeshAgent.transform;
        }

        Vector3 directionToPlayer = player.transform.position - npcTransform.position;
        directionToPlayer.y = 0f;
        if (directionToPlayer.sqrMagnitude <= 0.001f)
        {
            return;
        }

        npcTransform.rotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
    }

/// <summary>
/// Performs the resolve dialogue data action.
/// </summary>
    protected virtual DialogueData ResolveDialogueData()
    {
        AlternateDialogueSwap alternateDialogueSwap = GetComponent<AlternateDialogueSwap>();
        if (alternateDialogueSwap != null && alternateDialogueSwap.CurrentDialogue != null)
        {
            return alternateDialogueSwap.CurrentDialogue;
        }

        CriminalDialogueSwap dialogueSwap = GetComponent<CriminalDialogueSwap>();
        if (dialogueSwap != null && dialogueSwap.CurrentDialogue != null)
        {
            return dialogueSwap.CurrentDialogue;
        }

        return dialogueData;
    }

/// <summary>
/// Performs the stop movement for dialogue action.
/// </summary>
    private void StopMovementForDialogue()
    {
        RoamingNpc roamingNpc = GetComponentInParent<RoamingNpc>();
        if (roamingNpc != null)
        {
            roamingNpc.BeginDialogueInteraction();
            return;
        }

        NavMeshAgent navMeshAgent = GetComponentInParent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.updateRotation = false;
        if (navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }

        navMeshAgent.velocity = Vector3.zero;
    }
}
