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
