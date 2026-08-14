/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for LinearDialogueTemplate.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LinearDialogueTemplate", menuName = "Dialogue/Linear Dialogue Template")]
/// <summary>
/// Defines the template structure for linear dialogue sequences.
/// </summary>
public class LinearDialogueTemplate : ScriptableObject
{
    [Header("Core")]
    [SerializeField] private string dialogueId = "linear_dialogue";
    [SerializeField] private string speaker = "NPC";
    [SerializeField] private List<string> lines = new();

    [Header("Optional Final Confirm / Deny")]
    [SerializeField] private bool includeFinalDecision;
    [TextArea(2, 6)] [SerializeField] private string decisionLine = "Would you like to continue?";
    [SerializeField] private string confirmText = "Yes";
    [SerializeField] private string denyText = "No";
    [SerializeField] private DialogueActionType confirmActionType = DialogueActionType.None;
    [SerializeField] private string confirmActionValue = "";

/// <summary>
/// Performs the dialogue id action.
/// </summary>
    public string DialogueId => dialogueId;
/// <summary>
/// Performs the speaker action.
/// </summary>
    public string Speaker => speaker;
/// <summary>
/// Performs the lines action.
/// </summary>
    public IReadOnlyList<string> Lines => lines;

/// <summary>
/// Performs the include final decision action.
/// </summary>
    public bool IncludeFinalDecision => includeFinalDecision;
/// <summary>
/// Performs the decision line action.
/// </summary>
    public string DecisionLine => decisionLine;
/// <summary>
/// Performs the confirm text action.
/// </summary>
    public string ConfirmText => confirmText;
/// <summary>
/// Performs the deny text action.
/// </summary>
    public string DenyText => denyText;
/// <summary>
/// Performs the confirm action type action.
/// </summary>
    public DialogueActionType ConfirmActionType => confirmActionType;
/// <summary>
/// Performs the confirm action value action.
/// </summary>
    public string ConfirmActionValue => confirmActionValue;
}
