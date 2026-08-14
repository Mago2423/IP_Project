/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Defines reusable dialogue assets, dialogue nodes, choices, and actions
/// that can be executed by the dialogue system.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
/// <summary>Stores the nodes and metadata for a dialogue conversation.</summary>
public class DialogueData : ScriptableObject
{
    [SerializeField] private string dialogueId = "new_dialogue";
    [SerializeField] private List<DialogueNode> nodes = new();

    /// <summary>Gets the unique identifier for this dialogue asset.</summary>
    public string DialogueId => dialogueId;

    /// <summary>Gets the ordered nodes contained in this dialogue asset.</summary>
    public IReadOnlyList<DialogueNode> Nodes => nodes;

    /// <summary>
    /// Gets the preferred starting node, or the first node when no preference is provided.
    /// </summary>
    /// <param name="preferredNodeId">Optional node identifier to use as the starting point.</param>
    /// <returns>The selected starting node, or null when the asset has no nodes.</returns>
    public DialogueNode GetStartNode(string preferredNodeId = "")
    {
        if (!string.IsNullOrWhiteSpace(preferredNodeId))
        {
            DialogueNode preferred = GetNode(preferredNodeId);
            if (preferred != null)
            {
                return preferred;
            }
        }

        return nodes.Count > 0 ? nodes[0] : null;
    }

    /// <summary>Finds a node by its identifier.</summary>
    /// <param name="nodeId">Identifier of the node to find.</param>
    /// <returns>The matching node, or null when no match exists.</returns>
    public DialogueNode GetNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        return nodes.Find(node => string.Equals(node.NodeId, nodeId, StringComparison.Ordinal));
    }
}

/// <summary>Actions that can be triggered by a dialogue choice.</summary>
public enum DialogueActionType
{
    /// <summary>Performs no action.</summary>
    None,
    /// <summary>Loads the scene named by the action value.</summary>
    LoadScene,
    /// <summary>Runs the teleporter on the source interactor.</summary>
    RunTeleporterOnSource,
    /// <summary>Starts the interrogation sequence on the source interactor.</summary>
    StartInterrogationOnSource,
    /// <summary>Collects evidence through the source interactor.</summary>
    CollectEvidenceOnSource,
    /// <summary>Selects a scam through the source interactor.</summary>
    SelectScamOnSource,
    /// <summary>Attempts an accusation through the source interactor.</summary>
    AttemptAccusationOnSource,
    /// <summary>Starts a new game.</summary>
    StartGame,
    /// <summary>Returns to the main menu.</summary>
    ReturnToMainMenu,
    /// <summary>Quits the application.</summary>
    QuitGame
}

/// <summary>Provides the source object associated with a dialogue action.</summary>
public readonly struct DialogueActionContext
{
    /// <summary>Creates an action context for a source interactor.</summary>
    /// <param name="sourceInteractor">Object that initiated the dialogue action.</param>
    public DialogueActionContext(MonoBehaviour sourceInteractor)
    {
        SourceInteractor = sourceInteractor;
    }

    /// <summary>Gets the object that initiated the action.</summary>
    public MonoBehaviour SourceInteractor { get; }
}

[Serializable]
/// <summary>Represents an action that can be executed by a dialogue choice.</summary>
public class DialogueAction
{
    [SerializeField] private DialogueActionType actionType;
    [SerializeField] private string stringValue = "";

    /// <summary>Gets the type of action to execute.</summary>
    public DialogueActionType ActionType => actionType;

    /// <summary>Gets the optional string value used by the action.</summary>
    public string StringValue => stringValue;

    /// <summary>Executes this action using the supplied dialogue context.</summary>
    /// <param name="context">Context containing the source interactor.</param>
    public void Execute(DialogueActionContext context)
    {
        switch (actionType)
        {
            case DialogueActionType.None:
                return;
            case DialogueActionType.LoadScene:
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return;
                }

                SceneManager.LoadScene(stringValue, LoadSceneMode.Single);
                return;
            case DialogueActionType.RunTeleporterOnSource:
                if (context.SourceInteractor == null)
                {
                    return;
                }

                if (!context.SourceInteractor.TryGetComponent(out TeleporterScript teleporter))
                {
                    return;
                }

                teleporter.Interact();
                return;
            case DialogueActionType.StartInterrogationOnSource:
                if (context.SourceInteractor == null)
                {
                    return;
                }

                if (!context.SourceInteractor.TryGetComponent(out OfficerRInterrogation interrogationSequence))
                {
                    return;
                }

                interrogationSequence.BeginSequence();
                return;
            case DialogueActionType.CollectEvidenceOnSource:
                if (context.SourceInteractor == null)
                {
                    return;
                }

                if (!context.SourceInteractor.TryGetComponent(out CaseFlowInteractable caseFlowInteractable))
                {
                    return;
                }

                caseFlowInteractable.Interact();
                return;
            case DialogueActionType.SelectScamOnSource:
                if (context.SourceInteractor == null)
                {
                    return;
                }

                if (!context.SourceInteractor.TryGetComponent(out CaseFlowInteractable scanInteractable))
                {
                    return;
                }

                scanInteractable.Interact();
                return;
            case DialogueActionType.AttemptAccusationOnSource:
                if (context.SourceInteractor == null)
                {
                    return;
                }

                if (!context.SourceInteractor.TryGetComponent(out CaseFlowInteractable accusationInteractable))
                {
                    return;
                }

                accusationInteractable.Interact();
                return;
            case DialogueActionType.StartGame:
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.StartNewGame();
                }

                return;
            case DialogueActionType.ReturnToMainMenu:
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.ReturnToMainMenu();
                }

                return;
            case DialogueActionType.QuitGame:
                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.QuitGame();
                }

                return;
            default:
                return;
        }
    }
}

[Serializable]
/// <summary>Represents one line and transition point in a dialogue.</summary>
public class DialogueNode
{
    [SerializeField] private string nodeId = "node_001";
    [SerializeField] private string speaker = "NPC";
    [TextArea(2, 6)] [SerializeField] private string line = "";
    [SerializeField] private bool endConversation;
    [SerializeField] private List<DialogueChoice> choices = new();
    [SerializeField] private string nextNodeId = "";

    /// <summary>Gets the unique identifier for this node.</summary>
    public string NodeId => nodeId;

    /// <summary>Gets the name of the character speaking this node.</summary>
    public string Speaker => speaker;

    /// <summary>Gets the line displayed for this node.</summary>
    public string Line => line;

    /// <summary>Gets whether this node ends the conversation.</summary>
    public bool EndConversation => endConversation;

    /// <summary>Gets the choices available from this node.</summary>
    public IReadOnlyList<DialogueChoice> Choices => choices;

    /// <summary>Gets the identifier of the next node for linear dialogue.</summary>
    public string NextNodeId => nextNodeId;

    /// <summary>Gets whether this node has one or more choices.</summary>
    public bool HasChoices => choices != null && choices.Count > 0;
}

[Serializable]
/// <summary>Represents a selectable response from a dialogue node.</summary>
public class DialogueChoice
{
    [SerializeField] private string text = "Choice";
    [SerializeField] private string nextNodeId = "";
    [SerializeField] private DialogueAction action = new();

    /// <summary>Gets the text displayed for this choice.</summary>
    public string Text => text;

    /// <summary>Gets the identifier of the node entered after this choice.</summary>
    public string NextNodeId => nextNodeId;

    /// <summary>Gets the action executed when this choice is selected.</summary>
    public DialogueAction Action => action;
}
