using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private GameFlowManager flowManager;

    private bool _isPaused;

    public bool IsPaused => _isPaused;

    private void Awake()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (flowManager == null)
        {
            flowManager = GameFlowManager.Instance;
        }

        ClosePauseMenu();
    }

    private void LateUpdate()
    {
        if (_isPaused && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    // Called by PlayerInput when the Pause action is performed.
    public void OnPause()
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (_isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        ShowPausePanel();
        SetPlayerCameraLocked(false);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        ClosePauseMenu();
        SetPlayerCameraLocked(true);
    }

    public void ShowHowToPlay()
    {
        SetPanelState(pause: false, howToPlay: true, settings: false);
    }

    public void ShowSettings()
    {
        SetPanelState(pause: false, howToPlay: false, settings: true);
    }

    public void ShowPausePanel()
    {
        SetPanelState(pause: true, howToPlay: false, settings: false);
    }

    public void RestartCase()
    {
        Time.timeScale = 1f;
        ResolveFlowManager();
        flowManager?.StartNewGame();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ResolveFlowManager();
        flowManager?.ReturnToMainMenu();
    }

    private void ClosePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void SetPanelState(bool pause, bool howToPlay, bool settings)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(pause);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(howToPlay);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(settings);
        }
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

    private void SetPlayerCameraLocked(bool isLocked)
    {
        if (player != null)
        {
            player.SetCameraLock(isLocked);
        }
    }
}
