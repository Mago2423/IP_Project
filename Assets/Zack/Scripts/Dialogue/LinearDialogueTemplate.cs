using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LinearDialogueTemplate", menuName = "Dialogue/Linear Dialogue Template")]
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

    public string DialogueId => dialogueId;
    public string Speaker => speaker;
    public IReadOnlyList<string> Lines => lines;

    public bool IncludeFinalDecision => includeFinalDecision;
    public string DecisionLine => decisionLine;
    public string ConfirmText => confirmText;
    public string DenyText => denyText;
    public DialogueActionType ConfirmActionType => confirmActionType;
    public string ConfirmActionValue => confirmActionValue;
}
