/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Controls the player's click-to-move navigation, interaction detection,
/// dialogue movement lock, and camera behavior in the Virtual World scene.
/// </summary>
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the top-down VirtualWorld player, including click-to-move navigation,
/// nearby interaction detection, and the fixed-angle camera.
/// </summary>
public class Controller : MonoBehaviour
{
    [Header("References")]
    /// <summary>
    /// Camera used to convert mouse positions into world raycasts.
    /// </summary>
    [SerializeField] private Camera mainCamera;
    /// <summary>
    /// Navigation agent that moves the VirtualWorld player.
    /// </summary>
    [SerializeField] private NavMeshAgent agent;
    /// <summary>
    /// Optional visual marker displayed at the selected destination.
    /// </summary>
    [SerializeField] private ClickMoveIndicator clickIndicator;
    /// <summary>
    /// Dialogue manager used to prevent movement during active dialogue.
    /// </summary>
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Camera Follow")]
    /// <summary>
    /// Whether the camera follows the player at a fixed angle.
    /// </summary>
    [SerializeField] private bool useFixedAngleCamera = true;
    /// <summary>
    /// Whether the camera is detached from the player hierarchy at startup.
    /// </summary>
    [SerializeField] private bool detachCameraOnStart = true;
    /// <summary>
    /// Camera offset from the player position.
    /// </summary>
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 14f, -10f);
    /// <summary>
    /// Fixed camera pitch in degrees.
    /// </summary>
    [SerializeField] private float cameraPitch = 45f;
    /// <summary>
    /// Fixed camera yaw in degrees.
    /// </summary>
    [SerializeField, HideInInspector] private float cameraYaw = 0f;
    /// <summary>
    /// Speed used to smooth the camera's movement toward the player.
    /// </summary>
    [SerializeField] private float cameraFollowSmooth = 12f;

    [Header("Camera Occlusion")]
    /// <summary>
    /// Whether the camera moves closer when an object blocks its view.
    /// </summary>
    [SerializeField, HideInInspector] private bool pullCameraWhenOccluded = false;
    /// <summary>
    /// Layers considered when checking for camera obstructions.
    /// </summary>
    [SerializeField, HideInInspector] private LayerMask occlusionLayers = ~0;
    /// <summary>
    /// Radius of the sphere cast used for camera obstruction checks.
    /// </summary>
    [SerializeField, HideInInspector] private float occlusionSphereRadius = 0.4f;
    /// <summary>
    /// Distance kept between the camera and an obstruction.
    /// </summary>
    [SerializeField, HideInInspector] private float occlusionPadding = 0.2f;
    /// <summary>
    /// Minimum distance allowed when the camera is pulled forward.
    /// </summary>
    [SerializeField, HideInInspector] private float minimumCameraDistance = 3f;
    /// <summary>
    /// Height on the player used as the camera occlusion focus point.
    /// </summary>
    [SerializeField, HideInInspector] private float occlusionFocusHeight = 1.2f;

    [Header("Movement Tuning")]
    /// <summary>
    /// Whether navigation-agent tuning is applied during initialization.
    /// </summary>
    [SerializeField, HideInInspector] private bool applyAgentTuning = true;
    /// <summary>
    /// Acceleration assigned to the navigation agent.
    /// </summary>
    [SerializeField, HideInInspector] private float tunedAcceleration = 24f;
    /// <summary>
    /// Angular rotation speed assigned to the navigation agent.
    /// </summary>
    [SerializeField, HideInInspector] private float tunedAngularSpeed = 720f;
    /// <summary>
    /// Stopping distance assigned to the navigation agent.
    /// </summary>
    [SerializeField, HideInInspector] private float tunedStoppingDistance = 0.05f;
    /// <summary>
    /// Whether the navigation agent brakes automatically at destinations.
    /// </summary>
    [SerializeField, HideInInspector] private bool tunedAutoBraking = false;

    [Header("Click Movement")]
    /// <summary>
    /// Minimum distance between destinations before a click is considered a retarget.
    /// </summary>
    [SerializeField, HideInInspector] private float minRetargetDistance = 0.35f;
    /// <summary>
    /// Minimum time between accepted movement clicks.
    /// </summary>
    [SerializeField, HideInInspector] private float clickRetargetCooldown = 0.02f;
    /// <summary>
    /// Whether the current path is cleared before a new destination is assigned.
    /// </summary>
    [SerializeField, HideInInspector] private bool hardStopOnRetarget = false;
    /// <summary>
    /// Distance used when sampling a clicked position on the navigation mesh.
    /// </summary>
    [SerializeField] private float navMeshClickSampleDistance = 0.25f;

    [Header("Interaction")]
    /// <summary>
    /// Maximum distance used when searching for nearby interactables.
    /// </summary>
    [SerializeField] private float interactRadius = 3f;
    /// <summary>
    /// Layers searched for nearby interactable colliders.
    /// </summary>
    [SerializeField, HideInInspector] private LayerMask interactLayers = ~0;

    /// <summary>
    /// Rotation applied to the fixed-angle camera.
    /// </summary>
    private Quaternion fixedCameraRotation;
    /// <summary>
    /// Earliest time at which another movement click can be accepted.
    /// </summary>
    private float nextAllowedRetargetTime;
    /// <summary>
    /// Reusable buffer for camera occlusion sphere-cast results.
    /// </summary>
    private readonly RaycastHit[] occlusionHits = new RaycastHit[16];
    /// <summary>
    /// Reusable buffer for nearby interaction overlap results.
    /// </summary>
    private readonly Collider[] interactOverlapHits = new Collider[32];
    /// <summary>
    /// Whether movement is currently blocked by dialogue.
    /// </summary>
    private bool dialogueMovementLocked;

    /// <summary>
    /// Resolves controller references, initializes the camera, and tunes the navigation agent.
    /// </summary>
    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (clickIndicator == null)
        {
            clickIndicator = FindFirstObjectByType<ClickMoveIndicator>();
        }

        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

        RefreshFixedCameraRotation();

        if (detachCameraOnStart && mainCamera != null)
        {
            mainCamera.transform.SetParent(null, true);
        }

        ApplyAgentTuning();
    }

    /// <summary>
    /// Processes click-to-move input and updates the navigation destination.
    /// </summary>
    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            StopAgentMovement(resetPath: true);
            return;
        }

        if (dialogueMovementLocked)
        {
            StopAgentMovement(resetPath: true);
            return;
        }

        if (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            return;
        }

        if (mainCamera == null || agent == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (Time.time < nextAllowedRetargetTime)
        {
            return;
        }

        if (!TryGetMouseHit(out RaycastHit hit))
        {
            return;
        }

        if (!TryGetNavMeshPoint(hit.point, out NavMeshHit navMeshHit))
        {
            return;
        }

        if (ShouldIgnoreRetarget(navMeshHit.position))
        {
            return;
        }

        SetDestination(navMeshHit.position, navMeshHit.normal);
    }

    /// <summary>
    /// Interacts with the object under the mouse or the nearest interactable in range.
    /// </summary>
    public void Interact()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            // Keep Interact for world interaction only while dialogue is open.
            return;
        }

        if (TryResolveInteractable(out IInteractable interactable))
        {
            StopAgentMovement(resetPath: true);
            interactable.Interact();
        }
    }

    /// <summary>
    /// Enables or disables movement while dialogue is active.
    /// </summary>
    /// <param name="isLocked">Whether movement should be locked.</param>
    public void SetDialogueMovementLocked(bool isLocked)
    {
        dialogueMovementLocked = isLocked;

        if (isLocked)
        {
            StopAgentMovement(resetPath: true);
        }
        else if (agent != null)
        {
            agent.updateRotation = true;
        }
    }

    /// <summary>
    /// Resolves an interactable by preferring the object under the cursor, then searching nearby.
    /// </summary>
    /// <param name="interactable">The resolved interactable, if one is found.</param>
    /// <returns>True when an interactable was resolved.</returns>
    private bool TryResolveInteractable(out IInteractable interactable)
    {
        interactable = null;

        if (mainCamera != null && TryGetMouseHit(out RaycastHit hit) && TryGetInteractable(hit.collider, out IInteractable raycastInteractable))
        {
            interactable = raycastInteractable;
            return true;
        }

        return TryGetNearestInteractableInRange(out interactable);
    }

    /// <summary>
    /// Stops the navigation agent and optionally clears its current path.
    /// </summary>
    /// <param name="resetPath">Whether the current navigation path should be cleared.</param>
    private void StopAgentMovement(bool resetPath)
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = true;
        agent.updateRotation = false;
        if (resetPath && agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }

    /// <summary>
    /// Finds the closest interactable collider within the configured interaction radius.
    /// </summary>
    /// <param name="interactable">The nearest interactable, if one is found.</param>
    /// <returns>True when an interactable is within range.</returns>
    private bool TryGetNearestInteractableInRange(out IInteractable interactable)
    {
        interactable = null;

        Vector3 origin = transform.position;
        int hitCount = Physics.OverlapSphereNonAlloc(origin, interactRadius, interactOverlapHits, interactLayers, QueryTriggerInteraction.Collide);
        if (hitCount <= 0)
        {
            return false;
        }

        float nearestDistanceSqr = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            Collider overlapCollider = interactOverlapHits[i];
            if (!TryGetInteractable(overlapCollider, out IInteractable candidate))
            {
                continue;
            }

            Vector3 candidatePoint = overlapCollider.ClosestPoint(origin);
            float distanceSqr = (candidatePoint - origin).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestDistanceSqr = distanceSqr;
            interactable = candidate;
        }

        return interactable != null;
    }

    /// <summary>
    /// Updates the fixed-angle camera position and rotation after movement.
    /// </summary>
    private void LateUpdate()
    {
        if (!useFixedAngleCamera || mainCamera == null)
        {
            return;
        }

        Vector3 targetPosition = GetDesiredCameraPosition();
        targetPosition = ResolveOccludedCameraPosition(targetPosition);
        float t = 1f - Mathf.Exp(-cameraFollowSmooth * Time.deltaTime);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, t);
        mainCamera.transform.rotation = fixedCameraRotation;
    }

    /// <summary>
    /// Applies the configured movement values to the navigation agent.
    /// </summary>
    private void ApplyAgentTuning()
    {
        if (!applyAgentTuning || agent == null)
        {
            return;
        }

        agent.acceleration = tunedAcceleration;
        agent.angularSpeed = tunedAngularSpeed;
        agent.stoppingDistance = tunedStoppingDistance;
        agent.autoBraking = tunedAutoBraking;
    }

    /// <summary>
    /// Raycasts from the camera through the current mouse position.
    /// </summary>
    /// <param name="hit">The collider hit by the mouse ray.</param>
    /// <returns>True when the mouse ray hits a collider.</returns>
    private bool TryGetMouseHit(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out hit);
    }

    /// <summary>
    /// Samples the navigation mesh near a clicked world position.
    /// </summary>
    /// <param name="clickedPoint">The world position selected by the mouse.</param>
    /// <param name="navMeshHit">The nearest valid navigation-mesh point.</param>
    /// <returns>True when a valid navigation-mesh point is found.</returns>
    private bool TryGetNavMeshPoint(Vector3 clickedPoint, out NavMeshHit navMeshHit)
    {
        navMeshHit = default;

        if (agent == null)
        {
            return false;
        }

        float sampleDistance = Mathf.Max(0.01f, navMeshClickSampleDistance);
        return NavMesh.SamplePosition(clickedPoint, out navMeshHit, sampleDistance, agent.areaMask);
    }

    /// <summary>
    /// Searches a collider and its parent hierarchy for an interactable component.
    /// Dialogue interactables are preferred when multiple components share an object.
    /// </summary>
    /// <param name="hitCollider">The collider to inspect.</param>
    /// <param name="interactable">The resolved interactable, if one is found.</param>
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
    /// Checks whether a new destination is too close to the agent's current destination.
    /// </summary>
    /// <param name="targetPoint">The proposed destination.</param>
    /// <returns>True when the destination should be ignored.</returns>
    private bool ShouldIgnoreRetarget(Vector3 targetPoint)
    {
        return agent.hasPath && (agent.destination - targetPoint).sqrMagnitude < minRetargetDistance * minRetargetDistance;
    }

    /// <summary>
    /// Sends the agent to a destination and displays the click indicator.
    /// </summary>
    /// <param name="targetPoint">The navigation-mesh destination.</param>
    /// <param name="hitNormal">The surface normal at the clicked location.</param>
    private void SetDestination(Vector3 targetPoint, Vector3 hitNormal)
    {
        if (hardStopOnRetarget)
        {
            StopAgentMovement(resetPath: true);
        }

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(targetPoint);

        if (clickIndicator != null)
        {
            clickIndicator.Show(targetPoint, hitNormal);
        }

        nextAllowedRetargetTime = Time.time + clickRetargetCooldown;
    }

    /// <summary>
    /// Rebuilds the camera rotation from the configured pitch and yaw.
    /// </summary>
    private void RefreshFixedCameraRotation()
    {
        fixedCameraRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }

    /// <summary>
    /// Calculates the camera's desired position relative to the player.
    /// </summary>
    /// <returns>The target camera position before occlusion adjustment.</returns>
    private Vector3 GetDesiredCameraPosition()
    {
        Vector3 rotatedOffset = Quaternion.Euler(0f, cameraYaw, 0f) * cameraOffset;
        return transform.position + rotatedOffset;
    }

    /// <summary>
    /// Pulls the camera toward the player when geometry blocks the desired position.
    /// </summary>
    /// <param name="desiredCameraPosition">The camera position before occlusion adjustment.</param>
    /// <returns>A camera position that avoids detected obstructions.</returns>
    private Vector3 ResolveOccludedCameraPosition(Vector3 desiredCameraPosition)
    {
        if (!pullCameraWhenOccluded)
        {
            return desiredCameraPosition;
        }

        Vector3 focusPosition = transform.position + Vector3.up * occlusionFocusHeight;
        Vector3 toCamera = desiredCameraPosition - focusPosition;
        float desiredDistance = toCamera.magnitude;

        if (desiredDistance <= 0.001f)
        {
            return desiredCameraPosition;
        }

        Vector3 direction = toCamera / desiredDistance;
        int hitCount = Physics.SphereCastNonAlloc(focusPosition, occlusionSphereRadius, direction, occlusionHits, desiredDistance, occlusionLayers, QueryTriggerInteraction.Ignore);

        float nearestValidHit = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = occlusionHits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            if (hitCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (occlusionHits[i].distance < nearestValidHit)
            {
                nearestValidHit = occlusionHits[i].distance;
            }
        }

        if (nearestValidHit == float.MaxValue)
        {
            return desiredCameraPosition;
        }

        float clampedDistance = Mathf.Max(minimumCameraDistance, nearestValidHit - occlusionPadding);
        clampedDistance = Mathf.Min(clampedDistance, desiredDistance);
        return focusPosition + direction * clampedDistance;
    }
}
