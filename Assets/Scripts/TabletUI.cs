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
