/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for PauseMenuUI.
/// </summary>

using UnityEngine;

/// <summary>
/// Provides the pause menu u i behavior used by the game systems.
/// </summary>
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

/// <summary>
/// Performs the is paused action.
/// </summary>
    public bool IsPaused => _isPaused;

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
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

/// <summary>
/// Updates the camera position after the player has moved.
/// </summary>
    private void LateUpdate()
    {
        if (_isPaused && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    // Called by PlayerInput when the Pause action is performed.
/// <summary>
/// Performs the on pause action.
/// </summary>
    public void OnPause()
    {
        TogglePause();
    }

/// <summary>
/// Performs the toggle pause action.
/// </summary>
    public void TogglePause()
    {
        if (_isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

/// <summary>
/// Performs the pause game action.
/// </summary>
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

/// <summary>
/// Performs the resume game action.
/// </summary>
    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        ClosePauseMenu();
        RestorePlayerCameraState();
    }

/// <summary>
/// Performs the show how to play action.
/// </summary>
    public void ShowHowToPlay()
    {
        SetPanelState(pause: false, howToPlay: true, settings: false);
    }

/// <summary>
/// Performs the show settings action.
/// </summary>
    public void ShowSettings()
    {
        SetPanelState(pause: false, howToPlay: false, settings: true);
    }

/// <summary>
/// Performs the show pause panel action.
/// </summary>
    public void ShowPausePanel()
    {
        SetPanelState(pause: true, howToPlay: false, settings: false);
    }

/// <summary>
/// Performs the restart case action.
/// </summary>
    public void RestartCase()
    {
        Time.timeScale = 1f;
        ResolveFlowManager();
        flowManager?.StartNewGame();
    }

/// <summary>
/// Returns the player to the main menu.
/// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ResolveFlowManager();
        flowManager?.ReturnToMainMenu();
    }

/// <summary>
/// Performs the close pause menu action.
/// </summary>
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

/// <summary>
/// Performs the set panel state action.
/// </summary>
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
/// Performs the set player camera locked action.
/// </summary>
    private void SetPlayerCameraLocked(bool isLocked)
    {
        if (player != null)
        {
            player.SetCameraLock(isLocked);
        }
    }

/// <summary>
/// Performs the restore player camera state action.
/// </summary>
    private void RestorePlayerCameraState()
    {
        if (player != null)
        {
            player.RestoreDefaultCameraState();
        }
    }
}
