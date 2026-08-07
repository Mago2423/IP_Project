using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private string startNodeId = "";
    [SerializeField] private bool oneShot;

    private bool _hasTriggered;

    public void Interact()
    {
        if (oneShot && _hasTriggered)
        {
            return;
        }

        DialogueData resolvedDialogue = ResolveDialogueData();
        if (resolvedDialogue == null)
        {
            Debug.LogWarning($"{nameof(DialogueTrigger)} on {name} is missing Dialogue Data.", this);
            return;
        }

        DialogueManager manager = FindFirstObjectByType<DialogueManager>();
        if (manager == null)
        {
            Debug.LogWarning("No DialogueManager found in scene.");
            return;
        }

        manager.StartDialogue(resolvedDialogue, startNodeId, this);
        _hasTriggered = true;
    }

    private DialogueData ResolveDialogueData()
    {
        CriminalDialogueSwap dialogueSwap = GetComponent<CriminalDialogueSwap>();
        if (dialogueSwap != null && dialogueSwap.CurrentDialogue != null)
        {
            return dialogueSwap.CurrentDialogue;
        }

        return dialogueData;
    }
}
