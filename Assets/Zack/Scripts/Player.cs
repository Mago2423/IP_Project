using UnityEngine;
using StarterAssets;

public class Player : MonoBehaviour
{
    public TabletUI tabletUI;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float raycastDistance = 10f;

    private StarterAssetsInputs _inputs;
    // hit info from the last UpdateRaycast call, available for other scripts to read
    public RaycastHit CurrentHit { get; private set; }
    public bool IsHitting { get; private set; }

    private void Awake()
    {
        _inputs = GetComponent<StarterAssetsInputs>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        UpdateRaycast();
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
        if (!IsHitting) return;

        if (CurrentHit.collider.TryGetComponent(out IInteractable interactable))
        {
            interactable.Interact();
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


}
