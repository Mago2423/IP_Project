/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for LinearDialogueTemplateBuilder.
/// </summary>

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds linear dialogue template assets for editor-driven content creation.
/// </summary>
public static class LinearDialogueTemplateBuilder
{
    [MenuItem("Tools/Dialogue/Create Linear Dialogue Template")]
/// <summary>
/// Creates a new dialogue template asset from the current editor selection.
/// </summary>
    public static void CreateTemplateAsset()
    {
        string targetFolder = GetSelectionFolderPath();
        LinearDialogueTemplate template = ScriptableObject.CreateInstance<LinearDialogueTemplate>();
        string templatePath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/LinearDialogueTemplate.asset");

        AssetDatabase.CreateAsset(template, templatePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = template;
        EditorGUIUtility.PingObject(template);
    }

    [MenuItem("Tools/Dialogue/Build DialogueData From Selected Linear Template")]
/// <summary>
/// Builds a generated dialogue asset from the selected template.
/// </summary>
    public static void BuildFromSelection()
    {
        LinearDialogueTemplate template = Selection.activeObject as LinearDialogueTemplate;
        if (template == null)
        {
            EditorUtility.DisplayDialog("Dialogue", "Select a LinearDialogueTemplate asset first.", "OK");
            return;
        }

        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(dialogue, "dialogueId", string.IsNullOrWhiteSpace(template.DialogueId) ? template.name : template.DialogueId);

        string speaker = string.IsNullOrWhiteSpace(template.Speaker) ? "NPC" : template.Speaker;
        List<DialogueNode> nodes = new();

        IReadOnlyList<string> lines = template.Lines;
        int lineCount = lines.Count;

        if (lineCount == 0 && !template.IncludeFinalDecision)
        {
            nodes.Add(CreateNode("start", speaker, string.Empty, string.Empty, true));
        }
        else
        {
            for (int i = 0; i < lineCount; i++)
            {
                bool isLastLine = i == lineCount - 1;
                string nodeId = i == 0 ? "start" : $"line_{i + 1}";

                string nextNodeId;
                bool endConversation;

                if (isLastLine)
                {
                    if (template.IncludeFinalDecision)
                    {
                        nextNodeId = "decision";
                        endConversation = false;
                    }
                    else
                    {
                        nextNodeId = string.Empty;
                        endConversation = true;
                    }
                }
                else
                {
                    nextNodeId = $"line_{i + 2}";
                    endConversation = false;
                }

                nodes.Add(CreateNode(nodeId, speaker, lines[i] ?? string.Empty, nextNodeId, endConversation));
            }

            if (lineCount == 0 && template.IncludeFinalDecision)
            {
                nodes.Add(CreateNode("start", speaker, template.DecisionLine ?? string.Empty, string.Empty, false, CreateDecisionChoices(template)));
            }
            else if (template.IncludeFinalDecision)
            {
                nodes.Add(CreateNode("decision", speaker, template.DecisionLine ?? string.Empty, string.Empty, false, CreateDecisionChoices(template)));
            }
        }

        SetPrivateField(dialogue, "nodes", nodes);

        string templatePath = AssetDatabase.GetAssetPath(template);
        string outputFolder = Path.GetDirectoryName(templatePath)?.Replace("\\", "/") ?? "Assets";
        string outputPath = $"{outputFolder}/{template.name}_Generated.asset";

        AssetDatabase.DeleteAsset(outputPath);
        AssetDatabase.CreateAsset(dialogue, outputPath);
        EditorUtility.SetDirty(dialogue);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = dialogue;
        EditorGUIUtility.PingObject(dialogue);

        EditorUtility.DisplayDialog("Dialogue", $"Generated: {outputPath}", "OK");
    }

    [MenuItem("Tools/Dialogue/Build DialogueData From Selected Linear Template", true)]
/// <summary>
/// Validates whether a dialogue template is currently selected.
/// </summary>
    private static bool ValidateBuildFromSelection()
    {
        return Selection.activeObject is LinearDialogueTemplate;
    }

/// <summary>
/// Performs the create decision choices action.
/// </summary>
    private static List<DialogueChoice> CreateDecisionChoices(LinearDialogueTemplate template)
    {
        List<DialogueChoice> choices = new();

        DialogueAction confirmAction = new DialogueAction();
        SetPrivateField(confirmAction, "actionType", template.ConfirmActionType);
        SetPrivateField(confirmAction, "stringValue", template.ConfirmActionValue ?? string.Empty);

        DialogueAction denyAction = new DialogueAction();
        SetPrivateField(denyAction, "actionType", DialogueActionType.None);
        SetPrivateField(denyAction, "stringValue", string.Empty);

        string confirmText = string.IsNullOrWhiteSpace(template.ConfirmText) ? "Yes" : template.ConfirmText;
        string denyText = string.IsNullOrWhiteSpace(template.DenyText) ? "No" : template.DenyText;

        choices.Add(CreateChoice(confirmText, string.Empty, confirmAction));
        choices.Add(CreateChoice(denyText, string.Empty, denyAction));

        return choices;
    }

/// <summary>
/// Creates a dialogue node definition for the generated dialogue asset.
/// </summary>
    private static DialogueNode CreateNode(
        string nodeId,
        string speaker,
        string line,
        string nextNodeId,
        bool endConversation = false,
        List<DialogueChoice> choices = null)
    {
        DialogueNode node = new DialogueNode();
        SetPrivateField(node, "nodeId", nodeId);
        SetPrivateField(node, "speaker", speaker);
        SetPrivateField(node, "line", line);
        SetPrivateField(node, "nextNodeId", nextNodeId);
        SetPrivateField(node, "endConversation", endConversation);
        SetPrivateField(node, "choices", choices ?? new List<DialogueChoice>());
        return node;
    }

/// <summary>
/// Creates a dialogue choice object for the generated dialogue asset.
/// </summary>
    private static DialogueChoice CreateChoice(string text, string nextNodeId, DialogueAction action)
    {
        DialogueChoice choice = new DialogueChoice();
        SetPrivateField(choice, "text", text);
        SetPrivateField(choice, "nextNodeId", nextNodeId);
        SetPrivateField(choice, "action", action);
        return choice;
    }

    private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
    {
        if (target == null)
        {
            return;
        }

        System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            return;
        }

        field.SetValue(target, value);
    }

/// <summary>
/// Performs the get selection folder path action.
/// </summary>
    private static string GetSelectionFolderPath()
    {
        Object selectedObject = Selection.activeObject;
        if (selectedObject == null)
        {
            return "Assets";
        }

        string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return "Assets";
        }

        if (AssetDatabase.IsValidFolder(selectedPath))
        {
            return selectedPath;
        }

        string directory = Path.GetDirectoryName(selectedPath);
        return string.IsNullOrWhiteSpace(directory) ? "Assets" : directory.Replace("\\", "/");
    }
}
