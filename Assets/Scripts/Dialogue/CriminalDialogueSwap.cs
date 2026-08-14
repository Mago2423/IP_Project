/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// <summary>
/// </summary>

using UnityEngine;

/// <summary>
/// Chooses between two dialogue assets for a criminal character.
/// </summary>
/// <summary>
/// Chooses the correct criminal dialogue branch based on investigation state.
/// </summary>
public class CriminalDialogueSwap : MonoBehaviour
{
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData evidenceDialogue;
    [SerializeField] private bool evidenceCollected;

    /// <summary>Gets whether the evidence dialogue should currently be used.</summary>
    public bool HasEvidenceDialogue => GameFlowManager.Instance != null
        ? GameFlowManager.Instance.HasRequiredEvidence
        : evidenceCollected;

    /// <summary>Gets the dialogue asset selected for the current evidence state.</summary>
    public DialogueData CurrentDialogue => HasEvidenceDialogue ? evidenceDialogue : introDialogue;

    /// <summary>
    /// Sets the local evidence state used when no game-flow manager is available.
    /// </summary>
    /// <param name="value">Whether the evidence dialogue should be selected.</param>
    public void SetEvidenceCollected(bool value)
    {
        evidenceCollected = value;
    }

    /// <summary>Forces the local selector to use the introductory dialogue.</summary>
    public void UseIntroDialogue()
    {
        evidenceCollected = false;
    }

    /// <summary>Forces the local selector to use the evidence dialogue.</summary>
    public void UseEvidenceDialogue()
    {
        evidenceCollected = true;
    }

    /// <summary>Gets the dialogue asset selected for the current state.</summary>
    public DialogueData GetCurrentDialogue()
    {
        return CurrentDialogue;
    }
}
