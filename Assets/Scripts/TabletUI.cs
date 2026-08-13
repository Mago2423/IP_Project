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
    public GameObject resultPanel;
    public TMP_Text resultText;    public TMP_Text selectedScamText;    public GameObject tablet;

    private void Start()
    {
        ShowPhishingPanel();
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
        ShowOnly(accusePanel);
    }

    public void UpdateSelectedScamText()
    {
        if (selectedScamText == null)
        {
            return;
        }

        if (GameFlowManager.Instance == null)
        {
            selectedScamText.text = "Selected scam: None";
            return;
        }

        GameFlowManager.ScamType selectedScam = GameFlowManager.Instance.SelectedScam;
        selectedScamText.text = selectedScam == GameFlowManager.ScamType.None
            ? "None"
            : selectedScam.ToString();
    }

    public void AttemptAccusation()
    {
        if (GameFlowManager.Instance != null)
        {
            bool accusationCorrect = GameFlowManager.Instance.AttemptAccusation();
            ShowResultPanel(accusationCorrect);
        }
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
        if (phishingPanel != null) phishingPanel.SetActive(panelToShow == phishingPanel);
        if (lotteryPanel != null) lotteryPanel.SetActive(panelToShow == lotteryPanel);
        if (romancePanel != null) romancePanel.SetActive(panelToShow == romancePanel);
        if (jobPanel != null) jobPanel.SetActive(panelToShow == jobPanel);
        if (investmentPanel != null) investmentPanel.SetActive(panelToShow == investmentPanel);
        if (donationPanel != null) donationPanel.SetActive(panelToShow == donationPanel);
        if (accusePanel != null) accusePanel.SetActive(panelToShow == accusePanel);
        if (resultPanel != null) resultPanel.SetActive(panelToShow == resultPanel);
    }
}
