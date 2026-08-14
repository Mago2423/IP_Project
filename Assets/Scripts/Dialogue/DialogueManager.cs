/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Controls active dialogue state, node progression, player dialogue mode,
/// and choice selection for the dialogue system.
/// </summary>
using UnityEngine;

/// <summary>Coordinates dialogue data, dialogue UI, and player interaction state.</summary>
public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private Player player;

    private DialogueData _activeDialogue;
    private DialogueNode _currentNode;
    private MonoBehaviour _activeSourceInteractor;

    /// <summary>Gets whether a valid dialogue node is currently active.</summary>
    public bool IsDialogueActive => _activeDialogue != null && _currentNode != null;

    /// <summary>Gets whether the current node requires a choice selection.</summary>
    public bool CurrentNodeHasChoices => IsDialogueActive && _currentNode.HasChoices;

    /// <summary>Gets the number of choices available on the current node.</summary>
    public int CurrentChoiceCount => CurrentNodeHasChoices ? _currentNode.Choices.Count : 0;

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
    private void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (dialogueView != null)
        {
            dialogueView.Hide();
        }
    }

    /// <summary>Starts a dialogue conversation from the selected starting node.</summary>
    /// <param name="dialogueData">Dialogue asset to display.</param>
    /// <param name="startNodeId">Optional identifier of the starting node.</param>
    /// <param name="sourceInteractor">Object that initiated the dialogue.</param>
    public void StartDialogue(DialogueData dialogueData, string startNodeId = "", MonoBehaviour sourceInteractor = null)
    {
        if (dialogueData == null)
        {
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        _activeDialogue = dialogueData;
        _activeSourceInteractor = sourceInteractor;
        _currentNode = _activeDialogue.GetStartNode(startNodeId);

        if (_currentNode == null)
        {
            EndDialogue();
            return;
        }

        if (player != null)
        {
            player.SetDialogueMode(true);
        }

        PresentCurrentNode();
    }

    /// <summary>Advances a linear conversation to its next node or ends it.</summary>
    public void Advance()
    {
        if (!IsDialogueActive)
        {
            return;
        }

        if (_currentNode.HasChoices)
        {
            return;
        }

        if (_currentNode.EndConversation)
        {
            EndDialogue();
            return;
        }

        if (string.IsNullOrWhiteSpace(_currentNode.NextNodeId))
        {
            EndDialogue();
            return;
        }

        MoveToNode(_currentNode.NextNodeId);
    }

    /// <summary>Selects a choice on the current node and executes its action.</summary>
    /// <param name="choiceIndex">Zero-based index of the selected choice.</param>
    public void SelectChoice(int choiceIndex)
    {
        if (!IsDialogueActive || !_currentNode.HasChoices)
        {
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= _currentNode.Choices.Count)
        {
            return;
        }

        DialogueChoice choice = _currentNode.Choices[choiceIndex];
        choice.Action?.Execute(new DialogueActionContext(_activeSourceInteractor));

        if (string.IsNullOrWhiteSpace(choice.NextNodeId))
        {
            EndDialogue();
            return;
        }

        MoveToNode(choice.NextNodeId);
    }

    /// <summary>Ends the active dialogue and restores player interaction.</summary>
    public void EndDialogue()
    {
        _activeDialogue = null;
        _currentNode = null;
        _activeSourceInteractor = null;

        if (dialogueView != null)
        {
            dialogueView.Hide();
        }

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (player != null)
        {
            player.SetDialogueMode(false);
        }
    }

/// <summary>
/// Moves the active dialogue to the specified node.
/// </summary>
    private void MoveToNode(string nodeId)
    {
        DialogueNode nextNode = _activeDialogue.GetNode(nodeId);
        if (nextNode == null)
        {
            EndDialogue();
            return;
        }

        _currentNode = nextNode;
        PresentCurrentNode();
    }

/// <summary>
/// Displays the current dialogue node in the dialogue UI.
/// </summary>
    private void PresentCurrentNode()
    {
        if (dialogueView == null)
        {
            EndDialogue();
            return;
        }

        dialogueView.ShowNode(_currentNode, Advance, SelectChoice);
    }
}
