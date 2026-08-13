using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Optional")]
    [SerializeField] private GameFlowManager flowManager;

    private void Awake()
    {
        if (flowManager == null)
        {
            flowManager = GameFlowManager.Instance;
        }

        ShowMainPanel();
    }

    public void StartInvestigation()
    {
        ResolveFlowManager();
        flowManager?.StartNewGame();
    }

    public void ShowHowToPlay()
    {
        SetPanelState(mainPanel: false, howToPlay: true, credits: false);
    }

    public void ShowCredits()
    {
        SetPanelState(mainPanel: false, howToPlay: false, credits: true);
    }

    public void ShowMainPanel()
    {
        SetPanelState(mainPanel: true, howToPlay: false, credits: false);
    }

    public void QuitGame()
    {
        ResolveFlowManager();
        flowManager?.QuitGame();
    }

    private void ResolveFlowManager()
    {
        if (flowManager == null)
        {
            flowManager = GameFlowManager.Instance != null
                ? GameFlowManager.Instance
                : FindFirstObjectByType<GameFlowManager>();
        }
    }

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
