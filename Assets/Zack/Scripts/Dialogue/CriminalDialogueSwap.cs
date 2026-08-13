using UnityEngine;

public class CriminalDialogueSwap : MonoBehaviour
{
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData evidenceDialogue;
    [SerializeField] private bool evidenceCollected;

    public bool HasEvidenceDialogue => GameFlowManager.Instance != null
        ? GameFlowManager.Instance.HasRequiredEvidence
        : evidenceCollected;

    public DialogueData CurrentDialogue => HasEvidenceDialogue ? evidenceDialogue : introDialogue;

    public void SetEvidenceCollected(bool value)
    {
        evidenceCollected = value;
    }

    public void UseIntroDialogue()
    {
        evidenceCollected = false;
    }

    public void UseEvidenceDialogue()
    {
        evidenceCollected = true;
    }

    public DialogueData GetCurrentDialogue()
    {
        return CurrentDialogue;
    }
}