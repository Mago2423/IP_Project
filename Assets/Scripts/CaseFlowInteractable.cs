using UnityEngine;

public class CaseFlowInteractable : MonoBehaviour, IInteractable
{
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

    public void Interact()
    {
        if (_hasUsed && actionType != ActionType.StartGame && actionType != ActionType.ReturnToMainMenu && actionType != ActionType.QuitGame && actionType != ActionType.ResetCase)
        {
            return;
        }

        GameFlowManager flowManager = GameFlowManager.Instance != null ? GameFlowManager.Instance : FindFirstObjectByType<GameFlowManager>();
        if (flowManager == null)
        {
            return;
        }

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

    public void OnClick()
    {
        Interact();
    }

    private void Execute(GameFlowManager flowManager)
    {
        switch (actionType)
        {
            case ActionType.CollectEvidence:
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