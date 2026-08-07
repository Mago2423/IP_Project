using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DialogueSampleAssetCreator
{
    private const string OutputFolder = "Assets/Zack/DialogueAssets";

    [MenuItem("Tools/Dialogue/Create Zack Sample Dialogue Assets")]
    public static void CreateSampleAssets()
    {
        EnsureFolderExists("Assets/Zack", "DialogueAssets");

        CreateOfficerRendellAsset();
        CreateCriminalJamalIntroAsset();
        CreateCriminalJamalEvidenceAsset();
        CreateDoctorStrangeAsset();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Dialogue", "Sample dialogue assets created in Assets/Zack/DialogueAssets.", "OK");
    }

    private static void CreateOfficerRendellAsset()
    {
        DialogueData asset = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(asset, "dialogueId", "officer_rendell_intro");

        List<DialogueNode> nodes = new()
        {
            CreateNode("start", "Officer Rendell", "Greetings player, My name is Officer Rendell and today we request your detective instinct to identify what scam this criminal has committed.", "movement"),
            CreateNode("movement", "Officer Rendell", "You can use WASD to move around the room. Press J to open up your tablet.", "tablet"),
            CreateNode("tablet", "Officer Rendell", "You may scroll through the tablet to look for the different types of scam that’s been more and more common.", "questions"),
            CreateNode("questions", "Officer Rendell", "You may ask questions to the criminal and try to see if he could shed light on his wrongdoing.", "leave_room"),
            CreateNode("leave_room", "Officer Rendell", "Once you’ve felt that you’re ready or have no further questions, you may press T to leave the interrogation room.", "help"),
            CreateNode("help", "Officer Rendell", "Now if there’s anything you are unsure of, I would be standing at the corner of the room. You may approach me anytime.", "", true)
        };

        SetPrivateField(asset, "nodes", nodes);
        SaveAsset(asset, "OfficerRendellIntro.asset");
    }

    private static void CreateCriminalJamalIntroAsset()
    {
        DialogueData asset = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(asset, "dialogueId", "criminal_jamal_intro");

        List<DialogueChoice> openingChoices = new()
        {
            CreateChoice("What scam have you committed?", "answer_1"),
            CreateChoice("What did you do?", "answer_2"),
            CreateChoice("What did you eat for lunch?", "answer_3"),
            CreateChoice("I am done questioning.", "")
        };

        List<DialogueNode> nodes = new()
        {
            CreateNode("start", "Criminal Jamal", "What do you want?", "", false, openingChoices),
            CreateNode("answer_1", "Criminal Jamal", "If I told you, it wouldn’t make it fun right?", "start"),
            CreateNode("answer_2", "Criminal Jamal", "I don’t know. What I do know is that it’s really easy to make elderly people fall prey when money’s involved.", "start"),
            CreateNode("answer_3", "Criminal Jamal", "Uhhhhh.... Mee Pok with Tomato Sauce from Hougang Block ABC", "start")
        };

        SetPrivateField(asset, "nodes", nodes);
        SaveAsset(asset, "CriminalJamalIntro.asset");
    }

    private static void CreateCriminalJamalEvidenceAsset()
    {
        DialogueData asset = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(asset, "dialogueId", "criminal_jamal_evidence");

        List<DialogueChoice> evidenceChoices = new()
        {
            CreateChoice("Did you make the elderly believe that they have won gifts, monopoly money and prizes?", "answer_1"),
            CreateChoice("Did you try to apply pressure and threaten to make the victim believe that they have to quickly give you their personal information for you to assist them?", "answer_2"),
            CreateChoice("Did you try to express strong feelings like 'I love you' or 'You're my soulmate' despite knowing the victim for such a short time?", "answer_3"),
            CreateChoice("I am done questioning.", "")
        };

        List<DialogueNode> nodes = new()
        {
            CreateNode("start", "Criminal Jamal", "What do you want?", "", false, evidenceChoices),
            CreateNode("answer_1", "Criminal Jamal", "Uhmmmm Maybe, I think I did. I don’t really remember.", "start"),
            CreateNode("answer_2", "Criminal Jamal", "WHAT, HOW DID YOU KNOW? I mean, I would never treat an elderly person like that, that would make them really stressed.", "start"),
            CreateNode("answer_3", "Criminal Jamal", "Huh? I have no idea what you’re talking about. I only have love for Lee Wei Goon. No one else.", "start")
        };

        SetPrivateField(asset, "nodes", nodes);
        SaveAsset(asset, "CriminalJamalEvidence.asset");
    }

    private static void CreateDoctorStrangeAsset()
    {
        DialogueData asset = ScriptableObject.CreateInstance<DialogueData>();
        SetPrivateField(asset, "dialogueId", "doctor_strange_exit");

        List<DialogueChoice> choices = new()
        {
            CreateChoice("Yes", "", CreateAction(DialogueActionType.RunTeleporterOnSource, "")),
            CreateChoice("Now", "")
        };

        List<DialogueNode> nodes = new()
        {
            CreateNode("start", "Doctor Strange", "Would you like to return to the mortal realm?", "", false, choices)
        };

        SetPrivateField(asset, "nodes", nodes);
        SaveAsset(asset, "DoctorStrangeExit.asset");
    }

    private static DialogueNode CreateNode(string nodeId, string speaker, string line, string nextNodeId, bool endConversation = false, List<DialogueChoice> choices = null)
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

    private static DialogueChoice CreateChoice(string text, string nextNodeId, DialogueAction action = null)
    {
        DialogueChoice choice = new DialogueChoice();
        SetPrivateField(choice, "text", text);
        SetPrivateField(choice, "nextNodeId", nextNodeId);
        SetPrivateField(choice, "action", action ?? CreateAction(DialogueActionType.None, ""));
        return choice;
    }

    private static DialogueAction CreateAction(DialogueActionType actionType, string stringValue)
    {
        DialogueAction action = new DialogueAction();
        SetPrivateField(action, "actionType", actionType);
        SetPrivateField(action, "stringValue", stringValue);
        return action;
    }

    private static void SaveAsset(DialogueData asset, string assetName)
    {
        string assetPath = $"{OutputFolder}/{assetName}";
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(asset, assetPath);
        EditorUtility.SetDirty(asset);
    }

    private static void EnsureFolderExists(string parentFolder, string childFolder)
    {
        string childPath = $"{parentFolder}/{childFolder}";
        if (!AssetDatabase.IsValidFolder(childPath))
        {
            AssetDatabase.CreateFolder(parentFolder, childFolder);
        }
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

    private static T GetPrivateField<T>(object source, string fieldName)
    {
        System.Reflection.FieldInfo field = source.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (T)field.GetValue(source) : default;
    }
}