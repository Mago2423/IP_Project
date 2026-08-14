/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for QuestionHubTemplateBuilder.
/// </summary>

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds question hub template assets for editor-driven content creation.
/// </summary>
public static class QuestionHubTemplateBuilder
{
    [MenuItem("Tools/Dialogue/Create Question Hub Template")]
/// <summary>
/// Creates a new dialogue template asset from the current editor selection.
/// </summary>
    public static void CreateTemplateAsset()
    {
        string targetFolder = GetSelectionFolderPath();
        QuestionHubTemplate template = ScriptableObject.CreateInstance<QuestionHubTemplate>();
        string templatePath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/QuestionHubTemplate.asset");

        AssetDatabase.CreateAsset(template, templatePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = template;
        EditorGUIUtility.PingObject(template);
    }

    [MenuItem("Tools/Dialogue/Build DialogueData From Selected Question Hub")]
/// <summary>
/// Builds a generated dialogue asset from the selected template.
/// </summary>
    public static void BuildFromSelection()
    {
        QuestionHubTemplate template = Selection.activeObject as QuestionHubTemplate;
        if (template == null)
        {
            EditorUtility.DisplayDialog("Dialogue", "Select a QuestionHubTemplate asset first.", "OK");
            return;
        }

        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(dialogue, "dialogueId", string.IsNullOrWhiteSpace(template.DialogueId) ? template.name : template.DialogueId);

        string startNodeId = string.IsNullOrWhiteSpace(template.StartNodeId) ? "start" : template.StartNodeId;
        string speaker = string.IsNullOrWhiteSpace(template.Speaker) ? "NPC" : template.Speaker;
        string openingLine = template.OpeningLine ?? string.Empty;

        List<DialogueChoice> startChoices = new();
        List<DialogueNode> nodes = new();

        IReadOnlyList<QuestionHubEntry> entries = template.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            QuestionHubEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            string answerNodeId = $"answer_{i + 1}";
            string questionText = string.IsNullOrWhiteSpace(entry.Question)
                ? $"Question {i + 1}"
                : entry.Question;

            startChoices.Add(CreateChoice(questionText, answerNodeId));
            nodes.Add(CreateNode(answerNodeId, speaker, entry.Answer ?? string.Empty, startNodeId));
        }

        startChoices.Add(CreateChoice(
            string.IsNullOrWhiteSpace(template.DoneChoiceText) ? "I am done questioning." : template.DoneChoiceText,
            string.Empty));

        nodes.Insert(0, CreateNode(startNodeId, speaker, openingLine, string.Empty, false, startChoices));

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

    [MenuItem("Tools/Dialogue/Build DialogueData From Selected Question Hub", true)]
/// <summary>
/// Validates whether a dialogue template is currently selected.
/// </summary>
    private static bool ValidateBuildFromSelection()
    {
        return Selection.activeObject is QuestionHubTemplate;
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
    private static DialogueChoice CreateChoice(string text, string nextNodeId)
    {
        DialogueChoice choice = new DialogueChoice();
        SetPrivateField(choice, "text", text);
        SetPrivateField(choice, "nextNodeId", nextNodeId);

        DialogueAction action = new DialogueAction();
        SetPrivateField(action, "actionType", DialogueActionType.None);
        SetPrivateField(action, "stringValue", string.Empty);

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
