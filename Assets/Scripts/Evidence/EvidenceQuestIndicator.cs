using UnityEngine;

public class EvidenceQuestIndicator : MonoBehaviour
{
    [Header("Indicator")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform indicatorAnchor;

    private GameObject _indicatorInstance;
    private DialogueInteractable _dialogueInteractable;

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

    private void Start()
    {
        SetIndicatorVisible(true);
    }

    private void OnEnable()
    {
        if (_dialogueInteractable != null)
        {
            _dialogueInteractable.DialogueStarted += HideIndicator;
        }
    }

    private void OnDisable()
    {
        if (_dialogueInteractable != null)
        {
            _dialogueInteractable.DialogueStarted -= HideIndicator;
        }
    }

    public void HideIndicator()
    {
        SetIndicatorVisible(false);
    }

    private void SetIndicatorVisible(bool isVisible)
    {
        if (_indicatorInstance != null)
        {
            _indicatorInstance.SetActive(isVisible);
        }
    }
}