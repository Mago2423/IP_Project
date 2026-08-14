/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Displays dialogue text, conversation prompts, and dynamically configured
/// response buttons for the dialogue system.
/// </summary>
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>Renders dialogue nodes and connects their UI controls to callbacks.</summary>
public class DialogueView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text lineText;

    [Header("Prompts")]
    [SerializeField] private GameObject nextPrompt;
    [SerializeField] private GameObject confirmPrompt;
    [SerializeField] private GameObject cancelPrompt;

    [Header("Mode Panels (Optional)")]
    [SerializeField] private GameObject linearModePanel;
    [SerializeField] private GameObject choiceModePanel;

    [Header("Choices - Premade")]
    [SerializeField] private List<Button> premadeChoiceButtons = new();

    private Action _onAdvance;
    private Action<int> _onChoiceSelected;

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
    private void Awake()
    {
        EnsureEventSystem();
        Hide();
    }

/// <summary>
/// Performs the ensure event system action.
/// </summary>
    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    /// <summary>
    /// Displays a dialogue node and binds callbacks for advancing or selecting choices.
    /// </summary>
    /// <param name="node">Dialogue node to display.</param>
    /// <param name="onAdvance">Callback invoked by the linear advance prompt.</param>
    /// <param name="onChoiceSelected">Callback invoked with the selected choice index.</param>
    public void ShowNode(DialogueNode node, Action onAdvance, Action<int> onChoiceSelected)
    {
        _onAdvance = onAdvance;
        _onChoiceSelected = onChoiceSelected;

        if (rootPanel != null)
        {
            rootPanel.SetActive(true);
        }

        if (speakerText != null)
        {
            speakerText.text = node.Speaker;
        }

        if (lineText != null)
        {
            lineText.text = node.Line;
        }

        SetModePanels(node);

        RebuildChoiceButtons(node);

        UpdatePromptVisibility(node);

        BindLinearPrompt();
    }

    /// <summary>Hides the dialogue UI and clears all configured choice listeners.</summary>
    public void Hide()
    {
        if (nextPrompt != null)
        {
            nextPrompt.SetActive(false);
        }

        if (confirmPrompt != null)
        {
            confirmPrompt.SetActive(false);
        }

        if (cancelPrompt != null)
        {
            cancelPrompt.SetActive(false);
        }

        SetModePanels(showLinear: false, showChoice: false);
        ClearChoices();

        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

/// <summary>
/// Performs the bind linear prompt action.
/// </summary>
    private void BindLinearPrompt()
    {
        Button button = GetPromptButton(nextPrompt);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onAdvance?.Invoke());
    }

/// <summary>
/// Performs the rebuild choice buttons action.
/// </summary>
    private void RebuildChoiceButtons(DialogueNode node)
    {
        ClearChoices();

        if (!node.HasChoices)
        {
            return;
        }

        if (node.Choices.Count == 2)
        {
            ConfigureTwoOptionPrompts(node);
            return;
        }

        if (TryShowWithPremadeButtons(node))
        {
            return;
        }
    }

/// <summary>
/// Performs the update prompt visibility action.
/// </summary>
    private void UpdatePromptVisibility(DialogueNode node)
    {
        bool isTwoOptionNode = node.HasChoices && node.Choices.Count == 2;

        if (nextPrompt != null)
        {
            nextPrompt.SetActive(!node.HasChoices);
        }

        if (confirmPrompt != null)
        {
            confirmPrompt.SetActive(isTwoOptionNode);
        }

        if (cancelPrompt != null)
        {
            cancelPrompt.SetActive(isTwoOptionNode);
        }
    }

/// <summary>
/// Performs the configure two option prompts action.
/// </summary>
    private void ConfigureTwoOptionPrompts(DialogueNode node)
    {
        if (confirmPrompt == null || cancelPrompt == null)
        {
            return;
        }

        SetupPromptButton(confirmPrompt, node, 0);
        SetupPromptButton(cancelPrompt, node, 1);
    }

/// <summary>
/// Performs the setup prompt button action.
/// </summary>
    private void SetupPromptButton(GameObject prompt, DialogueNode node, int choiceIndex)
    {
        if (prompt == null)
        {
            return;
        }

        Button button = GetPromptButton(prompt);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onChoiceSelected?.Invoke(choiceIndex));

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null && choiceIndex >= 0 && choiceIndex < node.Choices.Count)
        {
            label.text = node.Choices[choiceIndex].Text;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = 24f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

/// <summary>
/// Performs the get prompt button action.
/// </summary>
    private Button GetPromptButton(GameObject prompt)
    {
        if (prompt == null)
        {
            return null;
        }

        Button button = prompt.GetComponent<Button>();
        if (button != null)
        {
            return button;
        }

        return prompt.GetComponentInChildren<Button>(true);
    }

/// <summary>
/// Performs the set mode panels action.
/// </summary>
    private void SetModePanels(DialogueNode node)
    {
        bool hasChoices = node != null && node.HasChoices;
        bool isTwoOptionNode = hasChoices && node.Choices.Count == 2;
        bool isMultiChoiceNode = hasChoices && node.Choices.Count > 2;

        // Two-option prompts (Confirm/Cancel) are expected in the default/linear panel.
        SetModePanels(showLinear: !isMultiChoiceNode, showChoice: isMultiChoiceNode);
    }

/// <summary>
/// Performs the set mode panels action.
/// </summary>
    private void SetModePanels(bool showLinear, bool showChoice)
    {
        if (linearModePanel != null)
        {
            linearModePanel.SetActive(showLinear);
        }

        if (choiceModePanel != null)
        {
            choiceModePanel.SetActive(showChoice);
        }
    }

/// <summary>
/// Performs the try show with premade buttons action.
/// </summary>
    private bool TryShowWithPremadeButtons(DialogueNode node)
    {
        if (premadeChoiceButtons == null || premadeChoiceButtons.Count == 0)
        {
            return false;
        }

        bool useNumericPrefix = node.Choices.Count > 2;

        for (int i = 0; i < premadeChoiceButtons.Count; i++)
        {
            Button button = premadeChoiceButtons[i];
            if (button == null)
            {
                continue;
            }

            bool shouldShow = i < node.Choices.Count;
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(shouldShow);

            if (!shouldShow)
            {
                continue;
            }

            int capturedIndex = i;
            DialogueChoice choice = node.Choices[i];
            string displayText = useNumericPrefix
                ? $"{capturedIndex + 1}. {choice.Text}"
                : choice.Text;

            TMP_Text tmpButtonText = button.GetComponentInChildren<TMP_Text>();
            if (tmpButtonText != null)
            {
                tmpButtonText.text = displayText;
                tmpButtonText.enableAutoSizing = true;
                tmpButtonText.fontSizeMin = 12f;
                tmpButtonText.fontSizeMax = 24f;
                tmpButtonText.textWrappingMode = TextWrappingModes.NoWrap;
                tmpButtonText.overflowMode = TextOverflowModes.Ellipsis;
            }

            button.onClick.AddListener(() => _onChoiceSelected?.Invoke(capturedIndex));
        }

        return true;
    }

/// <summary>
/// Performs the clear choices action.
/// </summary>
    private void ClearChoices()
    {
        if (confirmPrompt != null)
        {
            Button confirmButton = GetPromptButton(confirmPrompt);
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
            }
        }

        if (cancelPrompt != null)
        {
            Button cancelButton = GetPromptButton(cancelPrompt);
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
            }
        }

        if (premadeChoiceButtons != null)
        {
            for (int i = 0; i < premadeChoiceButtons.Count; i++)
            {
                Button button = premadeChoiceButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }
    }
}
