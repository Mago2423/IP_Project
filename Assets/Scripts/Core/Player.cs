/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Spawns the return owl after the player has collected all required evidence
/// in the Virtual World scene.
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Coordinates player interaction, input callbacks, camera state, and dialogue-related locks.
/// This component supports both first-person scenes and the top-down VirtualWorld controller.
/// </summary>
/// <summary>
/// Handles player interactions, dialogue locks, and scene-specific gameplay logic.
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// Tablet UI opened by the journal input action.
    /// </summary>
    public TabletUI tabletUI;

    [Header("Interaction Prompt")]
    /// <summary>
    /// UI object shown when the player is looking at an interactable.
    /// </summary>
    [SerializeField] private GameObject interactPromptUI;
    /// <summary>
    /// Tag that can make an object eligible for the interaction prompt.
    /// </summary>
    [SerializeField] private string interactableTag = "Interactable";

    /// <summary>
    /// Camera used for first-person raycast interaction.
    /// </summary>
    [SerializeField] private Camera playerCamera;
    /// <summary>
    /// Maximum distance for the first-person interaction raycast.
    /// </summary>
    [SerializeField] private float raycastDistance = 10f;
    /// <summary>
    /// Dialogue manager used to track dialogue state and advance dialogue.
    /// </summary>
    [SerializeField] private DialogueManager dialogueManager;
    /// <summary>
    /// First-person movement controller disabled during dialogue.
    /// </summary>
    [SerializeField] private FirstPersonController firstPersonController;
    /// <summary>
    /// Third-person movement controller disabled during dialogue.
    /// </summary>
    [SerializeField] private ThirdPersonController thirdPersonController;
    /// <summary>
    /// Top-down VirtualWorld controller used for movement and interaction.
    /// </summary>
    [SerializeField] private Controller topDownController;

    /// <summary>
    /// Starter Assets input state used to control movement and cursor locking.
    /// </summary>
    private StarterAssetsInputs _inputs;
    /// <summary>
    /// Whether dialogue-specific movement and cursor rules are active.
    /// </summary>
    private bool _isDialogueMode;
    /// <summary>
    /// Hit information from the most recent interaction raycast.
    /// </summary>
    public RaycastHit CurrentHit { get; private set; }

    /// <summary>
    /// Gets whether the player camera is currently pointing at a collider.
    /// </summary>
    public bool IsHitting { get; private set; }

    /// <summary>
    /// Resolves scene references and initializes the interaction prompt.
    /// </summary>
    private void Awake()
    {
        _inputs = GetComponent<StarterAssetsInputs>();

        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
        }

        if (thirdPersonController == null)
        {
            thirdPersonController = GetComponent<ThirdPersonController>();
        }

        if (topDownController == null)
        {
            topDownController = GetComponent<Controller>();
        }

        SetInteractPromptVisible(false);
    }

    /// <summary>
    /// Applies the cursor policy for the active scene.
    /// </summary>
    private void Start()
    {
        SetCameraLock(!IsVirtualWorldScene());
    }

    /// <summary>
    /// Maintains dialogue input state and updates first-person interaction targeting.
    /// </summary>
    private void Update()
    {
        if (_isDialogueMode)
        {
            EnforceDialogueInputLock();
            EnsureDialogueCursorState();
        }

        UpdateRaycast();
        UpdateInteractPrompt();
    }

    /// <summary>
    /// Dialogue swap used to update Jamal's conversation after evidence is complete.
    /// </summary>
    public CriminalDialogueSwap jamalSwap;

    /// <summary>
    /// Switches Jamal's dialogue after the evidence collection sequence is complete.
    /// </summary>
    public void OnEvidenceComplete()
    {
        if (jamalSwap != null)
        {
            jamalSwap.UseEvidenceDialogue();
        }
    }

    /// <summary>
    /// Clears movement and look input while dialogue is active.
    /// </summary>
    private void EnforceDialogueInputLock()
    {
        if (_inputs == null)
        {
            return;
        }

        _inputs.MoveInput(Vector2.zero);
        _inputs.JumpInput(false);
        _inputs.SprintInput(false);
        _inputs.look = Vector2.zero;
    }

    /// <summary>
    /// Updates the latest camera raycast hit used by first-person interaction.
    /// </summary>
    private void UpdateRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        IsHitting = Physics.Raycast(ray, out RaycastHit hit, raycastDistance);
        CurrentHit = hit;
    }

    /// <summary>
    /// Shows the interaction prompt when the current target can be interacted with.
    /// </summary>
    private void UpdateInteractPrompt()
    {
        if (interactPromptUI == null)
        {
            return;
        }

        bool shouldShowPrompt = false;
        if (!_isDialogueMode && IsHitting)
        {
            shouldShowPrompt = TryGetInteractable(CurrentHit.collider, out _) || HasTagInHierarchy(CurrentHit.collider != null ? CurrentHit.collider.transform : null, interactableTag);
        }

        SetInteractPromptVisible(shouldShowPrompt);
    }

    /// <summary>
    /// Checks whether a transform or one of its parents has the requested tag.
    /// </summary>
    /// <param name="target">The transform at which to begin searching.</param>
    /// <param name="requiredTag">The tag to find.</param>
    /// <returns>True when the tag exists in the transform hierarchy.</returns>
    private bool HasTagInHierarchy(Transform target, string requiredTag)
    {
        if (target == null || string.IsNullOrWhiteSpace(requiredTag))
        {
            return false;
        }

        Transform current = target;
        while (current != null)
        {
            if (string.Equals(current.tag, requiredTag, System.StringComparison.Ordinal))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    /// <summary>
    /// Changes the interaction prompt's active state when necessary.
    /// </summary>
    /// <param name="isVisible">Whether the prompt should be visible.</param>
    private void SetInteractPromptVisible(bool isVisible)
    {
        if (interactPromptUI == null)
        {
            return;
        }

        if (interactPromptUI.activeSelf == isVisible)
        {
            return;
        }

        interactPromptUI.SetActive(isVisible);
    }

    /// <summary>
    /// Handles the camera-lock input action sent by PlayerInput.
    /// </summary>
    void OnCamaraLock()
    {
        if (_isDialogueMode)
        {
            return;
        }

        SetCameraLock();
    }

    /// <summary>
    /// Handles the interaction input action sent by PlayerInput.
    /// </summary>
    void OnInteract()
    {
        if (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            // Keep Interact for world interaction only while dialogue is open.
            return;
        }

        if (topDownController != null)
        {
            topDownController.Interact();
            return;
        }

        if (!IsHitting) return;

        if (TryGetInteractable(CurrentHit.collider, out IInteractable interactable))
        {
            interactable.Interact();
            return;
        }
    }

    /// <summary>
    /// Advances a linear dialogue when the dialogue system is waiting for input.
    /// </summary>
    void OnNext()
    {
        if (dialogueManager == null || !dialogueManager.IsDialogueActive)
        {
            return;
        }

        if (dialogueManager.CurrentNodeHasChoices)
        {
            return;
        }

        dialogueManager.Advance();
    }

    /// <summary>
    /// Finds an interactable on a collider or in its parent hierarchy.
    /// </summary>
    /// <param name="hitCollider">Collider to inspect.</param>
    /// <param name="interactable">Resolved interactable, if one exists.</param>
    /// <returns>True when an interactable is found.</returns>
    private static bool TryGetInteractable(Collider hitCollider, out IInteractable interactable)
    {
        interactable = null;

        if (hitCollider == null)
        {
            return false;
        }

        Transform current = hitCollider.transform;
        while (current != null)
        {
            // Prefer dialogue when multiple interactables exist on the same object.
            if (current.TryGetComponent(out DialogueInteractable dialogueInteractable))
            {
                interactable = dialogueInteractable;
                return true;
            }

            MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInteractable foundInteractable)
                {
                    interactable = foundInteractable;
                    return true;
                }
            }

            current = current.parent;
        }

        return false;
    }

    /// <summary>
    /// Opens or closes the tablet and updates the cursor state accordingly.
    /// </summary>
    void OnJournal()
    {
        if (tabletUI == null)
        {
            return;
        }

        tabletUI.OpenTablet();
        SetCameraLock(!tabletUI.IsOpen);
    }

    /// <summary>
    /// Forwards the pause input action to the scene's pause-menu UI.
    /// </summary>
    void OnPause()
    {
        PauseMenuUI pauseMenu = FindFirstObjectByType<PauseMenuUI>();
        if (pauseMenu != null)
        {
            pauseMenu.OnPause();
        }
    }

    /// <summary>
    /// Locks or unlocks the camera cursor state.
    /// </summary>
    /// <param name="lockCamera">True to lock the cursor, false to unlock it, or null to toggle the current state.</param>
    public void SetCameraLock(bool? lockCamera = null)
    {
        bool currentState = _inputs != null
            ? _inputs.cursorLocked
            : Cursor.lockState == CursorLockMode.Locked;
        bool newState = lockCamera ?? !currentState;

        if (_inputs != null)
        {
            _inputs.cursorLocked = newState;
            _inputs.cursorInputForLook = newState;
            // Flush any queued delta so the camera doesn't jump on the transition frame.
            _inputs.look = Vector2.zero;
        }

        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }

    /// <summary>
    /// Restores the cursor behavior expected by the active scene.
    /// </summary>
    public void RestoreDefaultCameraState()
    {
        SetCameraLock(!IsVirtualWorldScene());
    }

    /// <summary>
    /// Enables or disables dialogue mode and locks movement systems while dialogue is active.
    /// </summary>
    /// <param name="isActive">Whether dialogue mode should be enabled.</param>
    public void SetDialogueMode(bool isActive)
    {
        _isDialogueMode = isActive;

        if (_inputs != null)
        {
            _inputs.MoveInput(Vector2.zero);
            _inputs.JumpInput(false);
            _inputs.SprintInput(false);
            _inputs.look = Vector2.zero;
        }

        if (firstPersonController != null)
        {
            firstPersonController.enabled = !isActive;
        }

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = !isActive;
        }

        if (topDownController != null)
        {
            topDownController.SetDialogueMovementLocked(isActive);
        }

        if (isActive)
        {
            EnforceDialogueInputLock();
            EnsureDialogueCursorState();
        }

        // Dialogue always unlocks the mouse; after dialogue restore this scene's default.
        SetCameraLock(isActive ? false : !IsVirtualWorldScene());
    }

    /// <summary>
    /// Determines whether the active scene uses VirtualWorld's unlocked cursor behavior.
    /// </summary>
    /// <returns>True when the active scene is VirtualWorld.</returns>
    private static bool IsVirtualWorldScene()
    {
        return string.Equals(
            SceneManager.GetActiveScene().name,
            "VirtualWorld",
            System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Ensures that dialogue can be interacted with using an unlocked cursor.
    /// </summary>
    private static void EnsureDialogueCursorState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


}
