using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public enum ScamType
    {
        None,
        Phishing,
        Lottery,
        Romance,
        Job,
        Investment,
        Donation
    }

    public enum CaseState
    {
        MainMenu,
        InGame,
        ReadyToAccuse,
        Accused,
        Won,
        Lost
    }

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "TheOffice";
    [SerializeField] private string winSceneName = "";
    [SerializeField] private string loseSceneName = "";

    [Header("Case Rules")]
    [SerializeField] private int requiredEvidenceCount = 3;
    [SerializeField] private ScamType requiredScam = ScamType.Romance;

    private readonly HashSet<string> _collectedEvidence = new();
    private CaseState _currentState = CaseState.MainMenu;
    private ScamType _selectedScam = ScamType.None;

    public static GameFlowManager Instance { get; private set; }
    public event Action EvidenceChanged;
    public event Action ScamSelected;

    public int EvidenceCount => _collectedEvidence.Count;
    public int RequiredEvidenceCount => requiredEvidenceCount;
    public bool HasRequiredEvidence => EvidenceCount >= requiredEvidenceCount;
    public ScamType SelectedScam => _selectedScam;
    public CaseState CurrentState => _currentState;
    public bool CanAccuse => HasRequiredEvidence && _selectedScam != ScamType.None && _currentState != CaseState.Won && _currentState != CaseState.Lost;

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

    public void StartNewGame()
    {
        ResetCaseProgress();
        _currentState = CaseState.InGame;

        if (!string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
    }

    public void ReturnToMainMenu()
    {
        ResetCaseProgress();

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        _currentState = CaseState.MainMenu;
    }

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

    public void ResetCaseProgress()
    {
        _collectedEvidence.Clear();
        _selectedScam = ScamType.None;
        _currentState = CaseState.InGame;
        ScamSelected?.Invoke();
    }

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

    public bool TrySelectScam(string scamName)
    {
        if (!System.Enum.TryParse(scamName, true, out ScamType scamType))
        {
            return false;
        }

        SelectScam(scamType);
        return true;
    }

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

    public void MarkInGame()
    {
        if (_currentState == CaseState.Won || _currentState == CaseState.Lost)
        {
            return;
        }

        _currentState = HasRequiredEvidence ? CaseState.ReadyToAccuse : CaseState.InGame;
    }

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