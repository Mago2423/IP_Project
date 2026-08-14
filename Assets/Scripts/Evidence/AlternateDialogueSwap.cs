/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for AlternateDialogueSwap.
/// </summary>

using UnityEngine;

/// <summary>
/// Chooses between an opening dialogue and a later dialogue using case progress.
/// </summary>
/// <summary>
/// Swaps dialogue assets depending on the current investigation progress.
/// </summary>
public class AlternateDialogueSwap : MonoBehaviour
{
    [SerializeField] private DialogueData firstVisitDialogue;
    [SerializeField] private DialogueData afterEvidenceDialogue;

    /// <summary>
    /// Returns the dialogue that matches the current evidence progress.
    /// </summary>
    public DialogueData CurrentDialogue
    {
        get
        {
            bool hasEvidence = GameFlowManager.Instance != null && GameFlowManager.Instance.HasRequiredEvidence;
            return hasEvidence ? afterEvidenceDialogue : firstVisitDialogue;
        }
    }
}
