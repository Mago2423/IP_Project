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
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
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

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
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
            EndDialogue();
            return;
        }

        dialogueView.ShowNode(_currentNode, Advance, SelectChoice);
    }
}
