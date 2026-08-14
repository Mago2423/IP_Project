/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for MainMenuUI.
/// </summary>

using UnityEngine;

/// <summary>
/// Provides the main menu u i behavior used by the game systems.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Optional")]
    [SerializeField] private GameFlowManager flowManager;

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
    private void Awake()
    {
        if (flowManager == null)
        {
            flowManager = GameFlowManager.Instance;
        }

        ShowMainPanel();
    }

/// <summary>
/// Performs the start investigation action.
/// </summary>
    public void StartInvestigation()
    {
        ResolveFlowManager();
        flowManager?.StartNewGame();
    }

/// <summary>
/// Performs the show how to play action.
/// </summary>
    public void ShowHowToPlay()
    {
        SetPanelState(mainPanel: false, howToPlay: true, credits: false);
    }

/// <summary>
/// Performs the show credits action.
/// </summary>
    public void ShowCredits()
    {
        SetPanelState(mainPanel: false, howToPlay: false, credits: true);
    }

/// <summary>
/// Performs the show main panel action.
/// </summary>
    public void ShowMainPanel()
    {
        SetPanelState(mainPanel: true, howToPlay: false, credits: false);
    }

/// <summary>
/// Quits the game application.
/// </summary>
    public void QuitGame()
    {
        ResolveFlowManager();
        flowManager?.QuitGame();
    }

/// <summary>
/// Performs the resolve flow manager action.
/// </summary>
    private void ResolveFlowManager()
    {
        if (flowManager == null)
        {
            flowManager = GameFlowManager.Instance != null
                ? GameFlowManager.Instance
                : FindFirstObjectByType<GameFlowManager>();
        }
    }

/// <summary>
/// Performs the set panel state action.
/// </summary>
    private void SetPanelState(bool mainPanel, bool howToPlay, bool credits)
    {
        if (this.mainPanel != null)
        {
            this.mainPanel.SetActive(mainPanel);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(howToPlay);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(credits);
        }
    }
}
