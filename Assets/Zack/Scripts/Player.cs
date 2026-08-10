using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Player : MonoBehaviour
{
    public TabletUI tabletUI;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float raycastDistance = 10f;
    [SerializeField] private DialogueManager dialogueManager;

    private StarterAssetsInputs _inputs;
    private bool _isDialogueMode;
    // hit info from the last UpdateRaycast call, available for other scripts to read
    public RaycastHit CurrentHit { get; private set; }
    public bool IsHitting { get; private set; }

    private void Awake()
    {
        _inputs = GetComponent<StarterAssetsInputs>();

        if (_inputs == null)
        {
            Debug.LogWarning($"{nameof(Player)} on {name} is missing {nameof(StarterAssetsInputs)} component.", this);
        }

        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

        if (dialogueManager == null)
        {
            Debug.LogWarning($"{nameof(Player)} on {name} could not find a {nameof(DialogueManager)} in scene.", this);
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogWarning($"{nameof(Player)} on {name} is missing Player Camera reference.", this);
        }
    }

    private void Update()
    {
        if (_isDialogueMode)
        {
            EnforceDialogueInputLock();
        }

        UpdateRaycast();
    }

    public CriminalDialogueSwap jamalSwap;

    void OnEvidenceComplete()
    {
        jamalSwap.UseEvidenceDialogue();
    }

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

    private void UpdateRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        IsHitting = Physics.Raycast(ray, out RaycastHit hit, raycastDistance);
        CurrentHit = hit;
    }

    // called by PlayerInput via SendMessages when CamaraLock action fires
    void OnCamaraLock()
    {
        SetCameraLock();
    }

    // called by PlayerInput via SendMessages when Interact action fires
    void OnInteract()
    {
        if (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            if (dialogueManager.CurrentNodeHasChoices)
            {
                return;
            }

            dialogueManager.Advance();
            return;
        }

        if (!IsHitting) return;

        if (CurrentHit.collider.TryGetComponent(out IInteractable interactable))
        {
            interactable.Interact();
            return;
        }
    }
        void OnJournal()
    {
        if (tabletUI != null)
        {
            tabletUI.OpenTablet();
            // unlock camera when journal opens, re-lock when it closes
            SetCameraLock(!tabletUI.IsOpen);
        }
    }

    // pass true to lock camera (hide cursor), false to unlock (show cursor), or no arg to toggle
    public void SetCameraLock(bool? lockCamera = null)
    {
        if (_inputs == null) return;

        bool newState = lockCamera ?? !_inputs.cursorLocked;
        _inputs.cursorLocked = newState;
        _inputs.cursorInputForLook = newState;
        // flush any queued delta so the camera doesn't jump on the transition frame
        _inputs.look = Vector2.zero;
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }

    public void SetDialogueMode(bool isActive)
    {
        _isDialogueMode = isActive;

        if (isActive)
        {
            EnforceDialogueInputLock();
        }

        // Dialogue mode keeps the camera still and unlocks the mouse for UI interaction.
        SetCameraLock(!isActive);
    }


}
