using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        ValidateReferences();
        Hide();
    }

    private void ValidateReferences()
    {
        if (rootPanel == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} is missing Root Panel.", this);
        }

        if (speakerText == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} is missing Speaker Text.", this);
        }

        if (lineText == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} is missing Line Text.", this);
        }

        if (nextPrompt == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} is missing Next Prompt.", this);
        }

        if (confirmPrompt == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} is missing Confirm Prompt.", this);
        }

        if (cancelPrompt == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} is missing Cancel Prompt.", this);
        }

        if (linearModePanel == null || choiceModePanel == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} can use optional Linear/Choice mode panels for cleaner two-layout switching.", this);
        }

        if (premadeChoiceButtons == null || premadeChoiceButtons.Count == 0)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} needs Premade Choice Buttons assigned for branching dialogue.", this);
        }
        else
        {
            for (int i = 0; i < premadeChoiceButtons.Count; i++)
            {
                Button button = premadeChoiceButtons[i];
                if (button == null)
                {
                    Debug.LogWarning($"{nameof(DialogueView)} on {name} has a null entry in Premade Choice Buttons at index {i}.", this);
                    continue;
                }

                if (button.GetComponentInChildren<TMP_Text>() == null)
                {
                    Debug.LogWarning($"Premade choice button '{button.name}' on {name} is missing TMP_Text.", button);
                }
            }
        }
    }

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
    }

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
        Debug.LogWarning($"{nameof(DialogueView)} on {name} could not show choices because no premade buttons are configured.", this);
    }

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

    private void ConfigureTwoOptionPrompts(DialogueNode node)
    {
        if (confirmPrompt == null || cancelPrompt == null)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} needs both Confirm Prompt and Cancel Prompt for 2-option dialogue nodes.", this);
            return;
        }

        SetupPromptButton(confirmPrompt, node, 0);
        SetupPromptButton(cancelPrompt, node, 1);
    }

    private void SetupPromptButton(GameObject prompt, DialogueNode node, int choiceIndex)
    {
        if (prompt == null)
        {
            return;
        }

        Button button = prompt.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"Prompt '{prompt.name}' on {name} is missing a Button component.", prompt);
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onChoiceSelected?.Invoke(choiceIndex));

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null && choiceIndex >= 0 && choiceIndex < node.Choices.Count)
        {
            label.text = node.Choices[choiceIndex].Text;
        }
    }

    private void SetModePanels(DialogueNode node)
    {
        bool hasChoices = node != null && node.HasChoices;
        bool isTwoOptionNode = hasChoices && node.Choices.Count == 2;
        bool isMultiChoiceNode = hasChoices && node.Choices.Count > 2;

        // Two-option prompts (Confirm/Cancel) are expected in the default/linear panel.
        SetModePanels(showLinear: !isMultiChoiceNode, showChoice: isMultiChoiceNode);
    }

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

    private bool TryShowWithPremadeButtons(DialogueNode node)
    {
        if (premadeChoiceButtons == null || premadeChoiceButtons.Count == 0)
        {
            return false;
        }

        bool useNumericPrefix = node.Choices.Count > 2;

        if (node.Choices.Count > premadeChoiceButtons.Count)
        {
            Debug.LogWarning($"{nameof(DialogueView)} on {name} has {premadeChoiceButtons.Count} premade buttons but node '{node.NodeId}' needs {node.Choices.Count} choices.", this);
        }

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
            }
            else
            {
                Debug.LogWarning($"Premade choice button '{button.name}' on {name} is missing TMP_Text.", button);
            }

            button.onClick.AddListener(() => _onChoiceSelected?.Invoke(capturedIndex));
        }

        return true;
    }

    private void ClearChoices()
    {
        if (confirmPrompt != null)
        {
            Button confirmButton = confirmPrompt.GetComponent<Button>();
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
            }
        }

        if (cancelPrompt != null)
        {
            Button cancelButton = cancelPrompt.GetComponent<Button>();
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
