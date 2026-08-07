using UnityEngine;

public class CriminalDialogueSwap : MonoBehaviour
{
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData evidenceDialogue;
    [SerializeField] private bool evidenceCollected;

    public DialogueData CurrentDialogue => evidenceCollected ? evidenceDialogue : introDialogue;

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