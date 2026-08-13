using UnityEngine;

/// <summary>
/// Temporary test helper for running VirtualWorld directly from the Unity Editor.
///
/// The normal game creates GameFlowManager from the main menu. When VirtualWorld
/// is opened directly, that manager may not exist, so the evidence-gated owl would
/// never receive any evidence progress. This helper creates a temporary manager
/// and marks three test evidence items as collected.
///
/// Remove this component from the scene before making a final build if direct
/// scene testing is no longer needed.
/// </summary>
public class VirtualWorldTestBootstrap : MonoBehaviour
{
    /// <summary>
    /// Creates a temporary game-flow manager when the scene has none.
    /// </summary>
    private void Start()
    {
        if (GameFlowManager.Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("Temporary GameFlowManager");
        GameFlowManager manager = managerObject.AddComponent<GameFlowManager>();

        manager.CollectEvidence("test_evidence_1");
        manager.CollectEvidence("test_evidence_2");
        manager.CollectEvidence("test_evidence_3");
    }
}
