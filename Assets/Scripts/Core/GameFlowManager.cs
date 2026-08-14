/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// <summary>
/// </summary>

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks and controls the evidence and accusation progress for the current case.
/// </summary>
/// <summary>
/// Manages the investigation flow, evidence, accusations, and scene transitions.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    /// <summary>
    /// Scam categories that can be selected for the current accusation.
    /// </summary>
/// <summary>
/// Provides the scam type behavior used by the game systems.
/// </summary>
    public enum ScamType
    {
        /// <summary>No scam has been selected.</summary>
        None,
        /// <summary>A phishing scam.</summary>
        Phishing,
        /// <summary>A lottery scam.</summary>
        Lottery,
        /// <summary>A romance scam.</summary>
        Romance,
        /// <summary>A job scam.</summary>
        Job,
        /// <summary>An investment scam.</summary>
        Investment,
        /// <summary>A donation scam.</summary>
        Donation
    }

    /// <summary>
    /// Major states in the case and scene flow.
    /// </summary>
/// <summary>
/// Provides the case state behavior used by the game systems.
/// </summary>
    public enum CaseState
    {
        /// <summary>The main menu is active.</summary>
        MainMenu,
        /// <summary>The player is investigating the case.</summary>
        InGame,
        /// <summary>The player has enough evidence to make an accusation.</summary>
        ReadyToAccuse,
        /// <summary>The player has made an accusation.</summary>
        Accused,
        /// <summary>The player identified the correct scam.</summary>
        Won,
        /// <summary>The player made an incorrect or incomplete accusation.</summary>
        Lost
    }

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "";
    [SerializeField] private string gameplaySceneName = "";
    [SerializeField] private string winSceneName = "";
    [SerializeField] private string loseSceneName = "";

    [Header("Case Rules")]
    [SerializeField] private int requiredEvidenceCount = 3;
    [SerializeField] private ScamType requiredScam = ScamType.Romance;

    private readonly HashSet<string> _collectedEvidence = new();
    private CaseState _currentState = CaseState.MainMenu;
    private ScamType _selectedScam = ScamType.None;

    /// <summary>
    /// Persistent game-flow manager instance used by other systems.
    /// </summary>
    public static GameFlowManager Instance { get; private set; }

    /// <summary>Raised when the collected evidence changes.</summary>
    public event Action EvidenceChanged;

    /// <summary>Raised when the selected scam changes.</summary>
    public event Action ScamSelected;

    /// <summary>Gets the number of unique evidence items collected.</summary>
    public int EvidenceCount => _collectedEvidence.Count;

    /// <summary>Gets the number of evidence items required before accusing.</summary>
    public int RequiredEvidenceCount => requiredEvidenceCount;

    /// <summary>Gets whether enough evidence has been collected.</summary>
    public bool HasRequiredEvidence => EvidenceCount >= requiredEvidenceCount;

    /// <summary>Gets the scam currently selected by the player.</summary>
    public ScamType SelectedScam => _selectedScam;

    /// <summary>Gets the current case state.</summary>
    public CaseState CurrentState => _currentState;

    /// <summary>Gets whether the player can submit an accusation.</summary>
    public bool CanAccuse => HasRequiredEvidence && _selectedScam != ScamType.None && _currentState != CaseState.Won && _currentState != CaseState.Lost;

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RefreshStateFromScene();
    }

    /// <summary>
    /// Resets the current case and loads the gameplay scene.
    /// </summary>
    public void StartNewGame()
    {
        ResetCaseProgress();
        _currentState = CaseState.InGame;

        if (!string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Clears the current case and loads the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        ResetCaseProgress();

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        _currentState = CaseState.MainMenu;
    }

    /// <summary>
    /// Exits the application when running outside the Unity Editor.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        Application.Quit();
    }

    /// <summary>
    /// Clears collected evidence and resets the selected scam and case state.
    /// </summary>
    public void ResetCaseProgress()
    {
        _collectedEvidence.Clear();
        _selectedScam = ScamType.None;
        _currentState = CaseState.InGame;
        ScamSelected?.Invoke();
    }

    /// <summary>
    /// Adds a unique evidence item to the current case.
    /// </summary>
    /// <param name="evidenceId">Identifier for the evidence item; an ID is generated when omitted.</param>
    /// <returns>True when the evidence was added successfully.</returns>
    public bool CollectEvidence(string evidenceId = "")
    {
        string resolvedId = string.IsNullOrWhiteSpace(evidenceId) ? $"evidence_{_collectedEvidence.Count + 1}" : evidenceId.Trim();

        if (string.IsNullOrWhiteSpace(resolvedId))
        {
            Debug.LogWarning("CollectEvidence was called with an empty or whitespace evidence ID.");
            return false;
        }

        if (!_collectedEvidence.Add(resolvedId))
        {
            Debug.LogWarning($"Evidence already collected: {resolvedId}");
            return false;
        }

        Debug.Log($"Evidence collected: {resolvedId}. Total = {_collectedEvidence.Count}/{requiredEvidenceCount}");

        if (HasRequiredEvidence && _currentState == CaseState.InGame)
        {
            _currentState = CaseState.ReadyToAccuse;
        }

        EvidenceChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Selects the scam type the player intends to accuse.
    /// </summary>
    /// <param name="scamType">Scam category selected by the player.</param>
    public void SelectScam(ScamType scamType)
    {
        _selectedScam = scamType;
        ScamSelected?.Invoke();
        Debug.Log($"Scam selected: {_selectedScam}. Evidence={_collectedEvidence.Count}/{requiredEvidenceCount}. CanAccuse={CanAccuse}.");

        if (HasRequiredEvidence && _currentState == CaseState.InGame)
        {
            _currentState = CaseState.ReadyToAccuse;
        }
    }

    /// <summary>
    /// Attempts to select a scam type from its name.
    /// </summary>
    /// <param name="scamName">Scam name to parse, ignoring letter case.</param>
    /// <returns>True when the name matches a scam type.</returns>
    public bool TrySelectScam(string scamName)
    {
        if (!System.Enum.TryParse(scamName, true, out ScamType scamType))
        {
            return false;
        }

        SelectScam(scamType);
        return true;
    }

    /// <summary>
    /// Evaluates the selected scam and transitions to the win or lose state.
    /// </summary>
    /// <returns>True when the selected scam matches the required scam.</returns>
    public bool AttemptAccusation()
    {
        Debug.Log($"AttemptAccusation called. Evidence={_collectedEvidence.Count}/{requiredEvidenceCount}, SelectedScam={_selectedScam}, RequiredScam={requiredScam}, CurrentState={_currentState}, CanAccuse={CanAccuse}.");

        if (!CanAccuse)
        {
            Debug.LogWarning($"Accusation blocked. Evidence={_collectedEvidence.Count}/{requiredEvidenceCount}, SelectedScam={_selectedScam}, CurrentState={_currentState}");
            _currentState = CaseState.Lost;
            if (!string.IsNullOrWhiteSpace(loseSceneName))
            {
                SceneManager.LoadScene(loseSceneName, LoadSceneMode.Single);
            }

            return false;
        }

        bool isCorrectCase = _selectedScam == requiredScam;
        _currentState = isCorrectCase ? CaseState.Won : CaseState.Lost;

        string targetScene = isCorrectCase ? winSceneName : loseSceneName;
        if (!string.IsNullOrWhiteSpace(targetScene))
        {
            SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
        }

        return isCorrectCase;
    }

    /// <summary>
    /// Marks the current scene as an active investigation scene.
    /// </summary>
    public void MarkInGame()
    {
        if (_currentState == CaseState.Won || _currentState == CaseState.Lost)
        {
            return;
        }

        _currentState = HasRequiredEvidence ? CaseState.ReadyToAccuse : CaseState.InGame;
    }

/// <summary>
/// Performs the refresh state from scene action.
/// </summary>
    private void RefreshStateFromScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName) && string.Equals(activeSceneName, mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            _currentState = CaseState.MainMenu;
            return;
        }

        _currentState = HasRequiredEvidence ? CaseState.ReadyToAccuse : CaseState.InGame;
    }
}
