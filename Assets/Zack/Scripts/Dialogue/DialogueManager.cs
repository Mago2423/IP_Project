using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private Player player;

    private DialogueData _activeDialogue;
    private DialogueNode _currentNode;
    private MonoBehaviour _activeSourceInteractor;

    public bool IsDialogueActive => _activeDialogue != null && _currentNode != null;
    public bool CurrentNodeHasChoices => IsDialogueActive && _currentNode.HasChoices;
    public int CurrentChoiceCount => CurrentNodeHasChoices ? _currentNode.Choices.Count : 0;

    private void Awake()
    {
        if (dialogueView == null)
        {
            Debug.LogWarning($"{nameof(DialogueManager)} on {name} is missing DialogueView reference.", this);
        }

        if (player == null)
        {
            Debug.LogWarning($"{nameof(DialogueManager)} on {name} is missing Player reference. Dialogue mode lock/unlock will not run.", this);
        }

        if (dialogueView != null)
        {
            dialogueView.Hide();
        }
    }

    public void StartDialogue(DialogueData dialogueData, string startNodeId = "", MonoBehaviour sourceInteractor = null)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("DialogueManager received null dialogue data.");
            return;
        }

        _activeDialogue = dialogueData;
        _activeSourceInteractor = sourceInteractor;
        _currentNode = _activeDialogue.GetStartNode(startNodeId);

        if (_currentNode == null)
        {
            Debug.LogWarning($"Dialogue '{_activeDialogue.DialogueId}' has no valid start node.");
            EndDialogue();
            return;
        }

        if (player != null)
        {
            player.SetDialogueMode(true);
        }

        PresentCurrentNode();
    }

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

    public void SelectChoice(int choiceIndex)
    {
        if (!IsDialogueActive || !_currentNode.HasChoices)
        {
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= _currentNode.Choices.Count)
        {
            Debug.LogWarning($"Invalid dialogue choice index: {choiceIndex}");
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

    public void EndDialogue()
    {
        _activeDialogue = null;
        _currentNode = null;
        _activeSourceInteractor = null;

        if (dialogueView != null)
        {
            dialogueView.Hide();
        }

        if (player != null)
        {
            player.SetDialogueMode(false);
        }
    }

    private void MoveToNode(string nodeId)
    {
        DialogueNode nextNode = _activeDialogue.GetNode(nodeId);
        if (nextNode == null)
        {
            Debug.LogWarning($"Dialogue node '{nodeId}' was not found.");
            EndDialogue();
            return;
        }

        _currentNode = nextNode;
        PresentCurrentNode();
    }

    private void PresentCurrentNode()
    {
        if (dialogueView == null)
        {
            Debug.LogError("DialogueView reference is missing on DialogueManager.");
            EndDialogue();
            return;
        }

        dialogueView.ShowNode(_currentNode, Advance, SelectChoice);
    }
}
