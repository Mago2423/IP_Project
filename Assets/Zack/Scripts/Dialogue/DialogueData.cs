using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string dialogueId = "new_dialogue";
    [SerializeField] private List<DialogueNode> nodes = new();

    public string DialogueId => dialogueId;
    public IReadOnlyList<DialogueNode> Nodes => nodes;

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

    public DialogueNode GetNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        return nodes.Find(node => string.Equals(node.NodeId, nodeId, StringComparison.Ordinal));
    }
}

public enum DialogueActionType
{
    None,
    LoadScene,
    RunTeleporterOnSource
}

public readonly struct DialogueActionContext
{
    public DialogueActionContext(MonoBehaviour sourceInteractor)
    {
        SourceInteractor = sourceInteractor;
    }

    public MonoBehaviour SourceInteractor { get; }
}

[Serializable]
public class DialogueAction
{
    [SerializeField] private DialogueActionType actionType;
    [SerializeField] private string stringValue = "";

    public DialogueActionType ActionType => actionType;
    public string StringValue => stringValue;

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
            default:
                return;
        }
    }
}

[Serializable]
public class DialogueNode
{
    [SerializeField] private string nodeId = "node_001";
    [SerializeField] private string speaker = "NPC";
    [TextArea(2, 6)] [SerializeField] private string line = "";
    [SerializeField] private bool endConversation;
    [SerializeField] private List<DialogueChoice> choices = new();
    [SerializeField] private string nextNodeId = "";

    public string NodeId => nodeId;
    public string Speaker => speaker;
    public string Line => line;
    public bool EndConversation => endConversation;
    public IReadOnlyList<DialogueChoice> Choices => choices;
    public string NextNodeId => nextNodeId;

    public bool HasChoices => choices != null && choices.Count > 0;
}

[Serializable]
public class DialogueChoice
{
    [SerializeField] private string text = "Choice";
    [SerializeField] private string nextNodeId = "";
    [SerializeField] private DialogueAction action = new();

    public string Text => text;
    public string NextNodeId => nextNodeId;
    public DialogueAction Action => action;
}
