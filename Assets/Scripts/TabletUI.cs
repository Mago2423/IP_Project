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
    public GameObject tablet;

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
        ShowOnly(accusePanel);
    }

    public void AttemptAccusation()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.AttemptAccusation();
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
    }
}
