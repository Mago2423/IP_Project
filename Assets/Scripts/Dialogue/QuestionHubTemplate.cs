using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionHubTemplate", menuName = "Dialogue/Question Hub Template")]
public class QuestionHubTemplate : ScriptableObject
{
    [SerializeField] private string dialogueId = "question_hub";
    [SerializeField] private string startNodeId = "start";
    [SerializeField] private string speaker = "NPC";
    [TextArea(2, 6)] [SerializeField] private string openingLine = "What do you want?";
    [SerializeField] private string doneChoiceText = "I am done questioning.";
    [SerializeField] private List<QuestionHubEntry> entries = new();

    public string DialogueId => dialogueId;
    public string StartNodeId => startNodeId;
    public string Speaker => speaker;
    public string OpeningLine => openingLine;
    public string DoneChoiceText => doneChoiceText;
    public IReadOnlyList<QuestionHubEntry> Entries => entries;
}

[Serializable]
public class QuestionHubEntry
{
    [SerializeField] private string question = "Ask a question";
    [TextArea(2, 6)] [SerializeField] private string answer = "Answer";

    public string Question => question;
    public string Answer => answer;
}
