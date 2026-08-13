using System;
using UnityEngine;
using UnityEngine.AI;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private string startNodeId = "";
    [SerializeField] private bool oneShot;

    private bool _hasTriggered;

    public event Action DialogueStarted;

    public bool HasTriggered => _hasTriggered;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interact();
        }
    }
    public void Interact()
    {
        if (oneShot && _hasTriggered)
        {
            return;
        }

        StopMovementForDialogue();

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
