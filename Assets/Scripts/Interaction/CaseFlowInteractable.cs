/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for CaseFlowInteractable.
/// </summary>

using UnityEngine;

/// <summary>
/// Triggers the case flow actions used during investigation and menu interactions.
/// </summary>
public class CaseFlowInteractable : MonoBehaviour, IInteractable
{
/// <summary>
/// Provides the action type behavior used by the game systems.
/// </summary>
    public enum ActionType
    {
        CollectEvidence,
        SelectScam,
        AttemptAccusation,
        StartGame,
        ReturnToMainMenu,
        QuitGame,
        ResetCase
    }

    [Header("Action")]
    [SerializeField] private ActionType actionType;

    [Header("Evidence")]
    [SerializeField] private string evidenceId = "";

    [Header("Scam")]
    [SerializeField] private GameFlowManager.ScamType scamType = GameFlowManager.ScamType.None;
    [SerializeField] private string scamName = "";

    [Header("Behaviour")]
    [SerializeField] private bool disableAfterUse;
    [SerializeField] private GameObject objectToDisable;

    private bool _hasUsed;

/// <summary>
/// Handles the interaction trigger for this object.
/// </summary>
    public void Interact()
    {
        if (_hasUsed && actionType != ActionType.StartGame && actionType != ActionType.ReturnToMainMenu && actionType != ActionType.QuitGame && actionType != ActionType.ResetCase)
                                    {
            return;
        }

        GameFlowManager flowManager = GameFlowManager.Instance != null ? GameFlowManager.Instance : FindFirstObjectByType<GameFlowManager>();
        if (flowManager == null)
        {
            Debug.LogError($"CaseFlowInteractable on '{gameObject.name}' could not find a GameFlowManager instance.");
            return;
        }

        Debug.Log($"CaseFlowInteractable triggered on '{gameObject.name}' with action '{actionType}' and evidenceId '{evidenceId}'");

        Execute(flowManager);

        if (!disableAfterUse)
        {
            return;
        }

        _hasUsed = true;

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
            return;
        }

        gameObject.SetActive(false);
    }

/// <summary>
/// Invokes the interact action from the UI click callback.
/// </summary>
    public void OnClick()
    {
        Interact();
    }

/// <summary>
/// Invokes the interact action when the object is clicked.
/// </summary>
    private void OnMouseDown()
    {
        Interact();
    }

/// <summary>
/// Executes the selected case-flow action for the current interaction.
/// </summary>
    private void Execute(GameFlowManager flowManager)
    {
        switch (actionType)
        {
            case ActionType.CollectEvidence:
                if (string.IsNullOrWhiteSpace(evidenceId))
                {
                    Debug.LogError($"CollectEvidence action on '{gameObject.name}' has no evidenceId assigned in the Inspector.");
                }

                flowManager.CollectEvidence(evidenceId);
                return;
            case ActionType.SelectScam:
                if (!string.IsNullOrWhiteSpace(scamName) && flowManager.TrySelectScam(scamName))
                {
                    return;
                }

                flowManager.SelectScam(scamType);
                return;
            case ActionType.AttemptAccusation:
                flowManager.AttemptAccusation();
                return;
            case ActionType.StartGame:
                flowManager.StartNewGame();
                return;
            case ActionType.ReturnToMainMenu:
                flowManager.ReturnToMainMenu();
                return;
            case ActionType.QuitGame:
                flowManager.QuitGame();
                return;
            case ActionType.ResetCase:
                flowManager.ResetCaseProgress();
                return;
            default:
                return;
        }
    }
}
