using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ScamSelected -= HandleScamSelected;
            GameFlowManager.Instance.EvidenceChanged -= HandleEvidenceChanged;
        }
    }

    private void Start()
    {
        ShowPhishingPanel();
        UpdateSelectedScamText();
    }

    private void HandleScamSelected()
    {
        UpdateSelectedScamText();
        UpdateAccuseButtonState();
    }

    private void HandleEvidenceChanged()
    {
        UpdateEvidenceText();
        UpdateAccuseButtonState();
    }

    private void UpdateAccuseButtonState()
    {
        if (accuseButton == null)
        {
            return;
        }

        accuseButton.interactable = GameFlowManager.Instance != null && GameFlowManager.Instance.CanAccuse;
    }

    public void ShowPhishingPanel()
    {
        ShowOnly(phishingPanel);
    }

    public void ShowLotteryPanel()
    {
        ShowOnly(lotteryPanel);
    }

    public void ShowRomancePanel()
    {
        ShowOnly(romancePanel);
    }

    public void ShowJobPanel()
    {
        ShowOnly(jobPanel);
    }

    public void ShowInvestmentPanel()
    {
        ShowOnly(investmentPanel);
    }

    public void ShowDonationPanel()
    {
        ShowOnly(donationPanel);
    }

    public void ShowAccusePanel()
    {
        UpdateSelectedScamText();
        UpdateEvidenceText();
        UpdateAccuseButtonState();
        ShowOnly(accusePanel);
    }

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

    public void ConfirmAccusation()
    {
        _confirmationOpen = false;

        if (GameFlowManager.Instance != null)
        {
            bool accusationCorrect = GameFlowManager.Instance.AttemptAccusation();
            ShowResultPanel(accusationCorrect);
        }
    }

    public void CancelAccusation()
    {
        _confirmationOpen = false;
        ShowAccusePanel();
    }

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

    public void CloseTablet()
    {
        if (tablet != null)
        {
            tablet.SetActive(false);
        }
    }

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

    public bool IsOpen => tablet != null && tablet.activeSelf;

    public void OpenTablet()
    {
        tablet.SetActive(!tablet.activeSelf);
    }
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
