/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for QuestionHubTemplate.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionHubTemplate", menuName = "Dialogue/Question Hub Template")]
/// <summary>
/// Defines the template structure for question-based dialogue content.
/// </summary>
public class QuestionHubTemplate : ScriptableObject
{
    [SerializeField] private string dialogueId = "question_hub";
    [SerializeField] private string startNodeId = "start";
    [SerializeField] private string speaker = "NPC";
    [TextArea(2, 6)] [SerializeField] private string openingLine = "What do you want?";
    [SerializeField] private string doneChoiceText = "I am done questioning.";
    [SerializeField] private List<QuestionHubEntry> entries = new();

/// <summary>
/// Performs the dialogue id action.
/// </summary>
    public string DialogueId => dialogueId;
/// <summary>
/// Performs the start node id action.
/// </summary>
    public string StartNodeId => startNodeId;
/// <summary>
/// Performs the speaker action.
/// </summary>
    public string Speaker => speaker;
/// <summary>
/// Performs the opening line action.
/// </summary>
    public string OpeningLine => openingLine;
/// <summary>
/// Performs the done choice text action.
/// </summary>
    public string DoneChoiceText => doneChoiceText;
/// <summary>
/// Performs the entries action.
/// </summary>
    public IReadOnlyList<QuestionHubEntry> Entries => entries;
}

[Serializable]
/// <summary>
/// Provides the question hub entry behavior used by the game systems.
/// </summary>
public class QuestionHubEntry
{
    [SerializeField] private string question = "Ask a question";
    [TextArea(2, 6)] [SerializeField] private string answer = "Answer";

/// <summary>
/// Performs the question action.
/// </summary>
    public string Question => question;
/// <summary>
/// Performs the answer action.
/// </summary>
    public string Answer => answer;
}
