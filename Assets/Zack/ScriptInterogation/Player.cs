using UnityEngine;

public class Player : MonoBehaviour
{
    public TabletUI tabletUI;
    void OnJournal()
    {
        if (tabletUI != null)
        {
            tabletUI.OpenTablet();
        }
    }

}
