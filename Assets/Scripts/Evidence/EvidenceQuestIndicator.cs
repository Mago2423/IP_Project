/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for EvidenceQuestIndicator.
/// </summary>

using UnityEngine;

/// <summary>
/// Provides the visual quest indicator for active evidence-related objectives.
/// </summary>
public class EvidenceQuestIndicator : MonoBehaviour
{
    [Header("Indicator")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform indicatorAnchor;

    private GameObject _indicatorInstance;
    private DialogueInteractable _dialogueInteractable;

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
    private void Awake()
    {
        if (indicatorAnchor == null)
        {
            indicatorAnchor = transform;
        }

        if (indicatorPrefab != null)
        {
            _indicatorInstance = Instantiate(indicatorPrefab, indicatorAnchor, false);
            _indicatorInstance.name = $"QuestIndicator_{name}";
        }

        _dialogueInteractable = GetComponentInParent<DialogueInteractable>();
    }

/// <summary>
/// Initializes gameplay state when the script begins running.
/// </summary>
    private void Start()
    {
        SetIndicatorVisible(true);
    }

/// <summary>
/// Performs the on enable action.
/// </summary>
    private void OnEnable()
    {
        if (_dialogueInteractable != null)
        {
            _dialogueInteractable.DialogueStarted += HideIndicator;
        }
    }

/// <summary>
/// Performs the on disable action.
/// </summary>
    private void OnDisable()
    {
        if (_dialogueInteractable != null)
        {
            _dialogueInteractable.DialogueStarted -= HideIndicator;
        }
    }

/// <summary>
/// Performs the hide indicator action.
/// </summary>
    public void HideIndicator()
    {
        SetIndicatorVisible(false);
    }

/// <summary>
/// Performs the set indicator visible action.
/// </summary>
    private void SetIndicatorVisible(bool isVisible)
    {
        if (_indicatorInstance != null)
        {
            _indicatorInstance.SetActive(isVisible);
        }
    }
}
