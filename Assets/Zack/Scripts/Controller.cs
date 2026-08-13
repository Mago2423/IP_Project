using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private ClickMoveIndicator clickIndicator;
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Camera Follow")]
    [SerializeField] private bool useFixedAngleCamera = true;
    [SerializeField] private bool detachCameraOnStart = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 14f, -10f);
    [SerializeField] private float cameraPitch = 45f;
    [SerializeField, HideInInspector] private float cameraYaw = 0f;
    [SerializeField] private float cameraFollowSmooth = 12f;

    [Header("Camera Occlusion")]
    [SerializeField, HideInInspector] private bool pullCameraWhenOccluded = false;
    [SerializeField, HideInInspector] private LayerMask occlusionLayers = ~0;
    [SerializeField, HideInInspector] private float occlusionSphereRadius = 0.4f;
    [SerializeField, HideInInspector] private float occlusionPadding = 0.2f;
    [SerializeField, HideInInspector] private float minimumCameraDistance = 3f;
    [SerializeField, HideInInspector] private float occlusionFocusHeight = 1.2f;

    [Header("Movement Tuning")]
    [SerializeField, HideInInspector] private bool applyAgentTuning = true;
    [SerializeField, HideInInspector] private float tunedAcceleration = 24f;
    [SerializeField, HideInInspector] private float tunedAngularSpeed = 720f;
    [SerializeField, HideInInspector] private float tunedStoppingDistance = 0.05f;
    [SerializeField, HideInInspector] private bool tunedAutoBraking = false;

    [Header("Click Movement")]
    [SerializeField, HideInInspector] private float minRetargetDistance = 0.35f;
    [SerializeField, HideInInspector] private float clickRetargetCooldown = 0.02f;
    [SerializeField, HideInInspector] private bool hardStopOnRetarget = false;
    [SerializeField] private float navMeshClickSampleDistance = 0.25f;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 3f;
    [SerializeField, HideInInspector] private LayerMask interactLayers = ~0;

    private Quaternion fixedCameraRotation;
    private float nextAllowedRetargetTime;
    private readonly RaycastHit[] occlusionHits = new RaycastHit[16];
    private readonly Collider[] interactOverlapHits = new Collider[32];
    private bool dialogueMovementLocked;

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

    private void Update()
    {
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

    private void OnInteract()
    {
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

    public void SetDialogueMovementLocked(bool isLocked)
    {
        dialogueMovementLocked = isLocked;

        if (isLocked)
        {
            StopAgentMovement(resetPath: true);
        }
    }

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

    private void StopAgentMovement(bool resetPath)
    {
        if (agent == null)
        {
            return;
        }

        agent.isStopped = true;
        if (resetPath && agent.hasPath)
        {
            agent.ResetPath();
        }

        agent.velocity = Vector3.zero;
    }

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

    private bool TryGetMouseHit(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out hit);
    }

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

    private bool ShouldIgnoreRetarget(Vector3 targetPoint)
    {
        return agent.hasPath && (agent.destination - targetPoint).sqrMagnitude < minRetargetDistance * minRetargetDistance;
    }

    private void SetDestination(Vector3 targetPoint, Vector3 hitNormal)
    {
        if (hardStopOnRetarget)
        {
            StopAgentMovement(resetPath: true);
        }

        agent.isStopped = false;
        agent.SetDestination(targetPoint);

        if (clickIndicator != null)
        {
            clickIndicator.Show(targetPoint, hitNormal);
        }

        nextAllowedRetargetTime = Time.time + clickRetargetCooldown;
    }

    private void RefreshFixedCameraRotation()
    {
        fixedCameraRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }

    private Vector3 GetDesiredCameraPosition()
    {
        Vector3 rotatedOffset = Quaternion.Euler(0f, cameraYaw, 0f) * cameraOffset;
        return transform.position + rotatedOffset;
    }

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
