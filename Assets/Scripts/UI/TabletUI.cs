/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for TabletUI.
/// </summary>

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides the tablet u i behavior used by the game systems.
/// </summary>
public class TabletUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject phishingPanel;
    public GameObject lotteryPanel;
    public GameObject romancePanel;
    public GameObject jobPanel;
    public GameObject investmentPanel;
    public GameObject donationPanel;
    public GameObject accusePanel;
    public GameObject confirmationPanel;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public TMP_Text selectedScamText;
    public TMP_Text evidenceText;
    public Button accuseButton;
    public GameObject tablet;
    private bool _confirmationOpen;

/// <summary>
/// Performs the on enable action.
/// </summary>
    private void OnEnable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ScamSelected += HandleScamSelected;
            GameFlowManager.Instance.EvidenceChanged += HandleEvidenceChanged;
        }

        UpdateSelectedScamText();
        UpdateEvidenceText();
        UpdateAccuseButtonState();
    }

/// <summary>
/// Performs the on disable action.
/// </summary>
    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ScamSelected -= HandleScamSelected;
            GameFlowManager.Instance.EvidenceChanged -= HandleEvidenceChanged;
        }
    }

/// <summary>
/// Initializes gameplay state when the script begins running.
/// </summary>
    private void Start()
    {
        ShowPhishingPanel();
        UpdateSelectedScamText();
    }

/// <summary>
/// Performs the handle scam selected action.
/// </summary>
    private void HandleScamSelected()
    {
        UpdateSelectedScamText();
        UpdateAccuseButtonState();
    }

/// <summary>
/// Performs the handle evidence changed action.
/// </summary>
    private void HandleEvidenceChanged()
    {
        UpdateEvidenceText();
        UpdateAccuseButtonState();
    }

/// <summary>
/// Performs the update accuse button state action.
/// </summary>
    private void UpdateAccuseButtonState()
    {
        if (accuseButton == null)
        {
            return;
        }

        accuseButton.interactable = GameFlowManager.Instance != null && GameFlowManager.Instance.CanAccuse;
    }

/// <summary>
/// Performs the show phishing panel action.
/// </summary>
    public void ShowPhishingPanel()
    {
        ShowOnly(phishingPanel);
    }

/// <summary>
/// Performs the show lottery panel action.
/// </summary>
    public void ShowLotteryPanel()
    {
        ShowOnly(lotteryPanel);
    }

/// <summary>
/// Performs the show romance panel action.
/// </summary>
    public void ShowRomancePanel()
    {
        ShowOnly(romancePanel);
    }

/// <summary>
/// Performs the show job panel action.
/// </summary>
    public void ShowJobPanel()
    {
        ShowOnly(jobPanel);
    }

/// <summary>
/// Performs the show investment panel action.
/// </summary>
    public void ShowInvestmentPanel()
    {
        ShowOnly(investmentPanel);
    }

/// <summary>
/// Performs the show donation panel action.
/// </summary>
    public void ShowDonationPanel()
    {
        ShowOnly(donationPanel);
    }

/// <summary>
/// Performs the show accuse panel action.
/// </summary>
    public void ShowAccusePanel()
    {
        UpdateSelectedScamText();
        UpdateEvidenceText();
        UpdateAccuseButtonState();
        ShowOnly(accusePanel);
    }

/// <summary>
/// Performs the update evidence text action.
/// </summary>
    public void UpdateEvidenceText()
    {
        if (evidenceText == null)
        {
            return;
        }

        if (GameFlowManager.Instance == null)
        {
            evidenceText.text = "0 / 3";
            return;
        }

        evidenceText.text = GameFlowManager.Instance.EvidenceCount + " / " + GameFlowManager.Instance.RequiredEvidenceCount;
    }

/// <summary>
/// Performs the update selected scam text action.
/// </summary>
    public void UpdateSelectedScamText()
    {
        if (selectedScamText == null)
        {
            return;
        }

        if (GameFlowManager.Instance == null)
        {
            selectedScamText.text = "NONE";
            return;
        }

        GameFlowManager.ScamType selectedScam = GameFlowManager.Instance.SelectedScam;
        selectedScamText.text = selectedScam == GameFlowManager.ScamType.None
            ? "NONE"
            : selectedScam.ToString().ToUpper();
    }

/// <summary>
/// Attempts to complete the accusation process.
/// </summary>
    public void AttemptAccusation()
    {
        if (confirmationPanel != null)
        {
            if (!GameFlowManager.Instance || !GameFlowManager.Instance.CanAccuse)
            {
                return;
            }

            _confirmationOpen = true;
            ShowOnly(confirmationPanel);
            return;
        }

        if (GameFlowManager.Instance != null)
        {
            bool accusationCorrect = GameFlowManager.Instance.AttemptAccusation();
            ShowResultPanel(accusationCorrect);
        }
    }

/// <summary>
/// Performs the confirm accusation action.
/// </summary>
    public void ConfirmAccusation()
    {
        _confirmationOpen = false;

        if (GameFlowManager.Instance != null)
        {
            bool accusationCorrect = GameFlowManager.Instance.AttemptAccusation();
            ShowResultPanel(accusationCorrect);
        }
    }

/// <summary>
/// Performs the cancel accusation action.
/// </summary>
    public void CancelAccusation()
    {
        _confirmationOpen = false;
        ShowAccusePanel();
    }

/// <summary>
/// Performs the show result panel action.
/// </summary>
    public void ShowResultPanel(bool accusationCorrect)
    {
        if (resultText != null)
        {
            resultText.text = accusationCorrect
                ? "Correct accusation!"
                : "Incorrect accusation.";
        }

        ShowOnly(resultPanel);
    }

/// <summary>
/// Performs the close tablet action.
/// </summary>
    public void CloseTablet()
    {
        if (tablet != null)
        {
            tablet.SetActive(false);
        }
    }

/// <summary>
/// Performs the show current scam panel action.
/// </summary>
    public void ShowCurrentScamPanel()
    {
        if (GameFlowManager.Instance == null)
        {
            ShowPhishingPanel();
            return;
        }

        switch (GameFlowManager.Instance.SelectedScam)
        {
            case GameFlowManager.ScamType.Lottery:
                ShowLotteryPanel();
                return;
            case GameFlowManager.ScamType.Romance:
                ShowRomancePanel();
                return;
            case GameFlowManager.ScamType.Job:
                ShowJobPanel();
                return;
            case GameFlowManager.ScamType.Investment:
                ShowInvestmentPanel();
                return;
            case GameFlowManager.ScamType.Donation:
                ShowDonationPanel();
                return;
            case GameFlowManager.ScamType.Phishing:
            default:
                ShowPhishingPanel();
                return;
        }
    }

/// <summary>
/// Performs the is open action.
/// </summary>
    public bool IsOpen => tablet != null && tablet.activeSelf;

/// <summary>
/// Performs the open tablet action.
/// </summary>
    public void OpenTablet()
    {
        tablet.SetActive(!tablet.activeSelf);
    }
/// <summary>
/// Performs the show only action.
/// </summary>
    private void ShowOnly(GameObject panelToShow)
    {
        if (phishingPanel != null) phishingPanel.SetActive(panelToShow == phishingPanel || panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (lotteryPanel != null) lotteryPanel.SetActive(panelToShow == lotteryPanel || panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (romancePanel != null) romancePanel.SetActive(panelToShow == romancePanel || panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (jobPanel != null) jobPanel.SetActive(panelToShow == jobPanel || panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (investmentPanel != null) investmentPanel.SetActive(panelToShow == investmentPanel || panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (donationPanel != null) donationPanel.SetActive(panelToShow == donationPanel || panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (accusePanel != null) accusePanel.SetActive(panelToShow == accusePanel || panelToShow == confirmationPanel || panelToShow == resultPanel);
        if (confirmationPanel != null) confirmationPanel.SetActive(panelToShow == confirmationPanel);
        if (resultPanel != null) resultPanel.SetActive(panelToShow == resultPanel);
    }
}
