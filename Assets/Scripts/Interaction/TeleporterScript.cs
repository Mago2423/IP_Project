/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for TeleporterScript.
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Teleports the player between scenes with a loading transition and safety checks.
/// </summary>
public class TeleporterScript : MonoBehaviour, IInteractable
{
    [Header("Teleport Destination")]
    [SerializeField] private string targetSceneName;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenUI;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private float minimumLoadingScreenTime = 0.75f;

    private bool isTeleporting;

/// <summary>
/// Handles the interaction trigger for this object.
/// </summary>
    public void Interact()
    {
        if (isTeleporting)
        {
            return;
        }

        StartCoroutine(TeleportWithLoadingScreen());
    }

/// <summary>
/// Performs the teleport with loading screen action.
/// </summary>
    private System.Collections.IEnumerator TeleportWithLoadingScreen()
    {
        isTeleporting = true;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            yield break;
        }

        isTeleporting = true;

        if (loadingScreenUI != null)
        {
            loadingScreenUI.SetActive(true);
        }

        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 0f;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);

        if (asyncLoad == null)
        {
            isTeleporting = false;
            yield break;
        }

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }

            yield return null;
        }

        float timer = 0f;
        while (timer < minimumLoadingScreenTime)
        {
            timer += Time.unscaledDeltaTime;

            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = Mathf.Clamp01(timer / minimumLoadingScreenTime);
            }

            yield return null;
        }

        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 1f;
        }

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
