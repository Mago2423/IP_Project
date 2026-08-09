using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public Camera mainCamera;
    public NavMeshAgent agent;
    [SerializeField] private ClickMoveIndicator clickIndicator;

    [Header("Camera Follow")]
    [SerializeField] private bool useFixedAngleCamera = true;
    [SerializeField] private bool detachCameraOnStart = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 14f, -10f);
    [SerializeField] private float cameraPitch = 45f;
    [SerializeField] private float cameraYaw = 0f;
    [SerializeField] private float cameraFollowSmooth = 12f;

    [Header("Movement Tuning")]
    [SerializeField] private bool applyAgentTuning = true;
    [SerializeField] private float tunedAcceleration = 24f;
    [SerializeField] private float tunedAngularSpeed = 720f;
    [SerializeField] private float tunedStoppingDistance = 0.05f;
    [SerializeField] private bool tunedAutoBraking = false;

    [Header("Click Movement")]
    [SerializeField] private float minRetargetDistance = 0.35f;
    [SerializeField] private float clickRetargetCooldown = 0.02f;
    [SerializeField] private bool hardStopOnRetarget = false;

    private Quaternion fixedCameraRotation;
    private float nextAllowedRetargetTime;

    void Awake()
    {
        if (clickIndicator == null)
        {
            clickIndicator = FindObjectOfType<ClickMoveIndicator>();
        }

        fixedCameraRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);

        if (detachCameraOnStart && mainCamera != null)
        {
            mainCamera.transform.SetParent(null, true);
        }

        if (!applyAgentTuning || agent == null)
        {
            return;
        }

        agent.acceleration = tunedAcceleration;
        agent.angularSpeed = tunedAngularSpeed;
        agent.stoppingDistance = tunedStoppingDistance;
        agent.autoBraking = tunedAutoBraking;
    }

    void Update()
    {
        if (mainCamera == null || agent == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time < nextAllowedRetargetTime)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (agent.hasPath && (agent.destination - hit.point).sqrMagnitude < minRetargetDistance * minRetargetDistance)
                {
                    return;
                }

                if (hardStopOnRetarget)
                {
                    // Optional strict retargeting behavior for stop-then-go control feel.
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }

                agent.isStopped = false;
                agent.SetDestination(hit.point);
                if (clickIndicator != null)
                {
                    clickIndicator.Show(hit.point, hit.normal);
                }
                nextAllowedRetargetTime = Time.time + clickRetargetCooldown;
            }
        }
    }

    void LateUpdate()
    {
        if (!useFixedAngleCamera || mainCamera == null)
        {
            return;
        }

        Vector3 targetPosition = transform.position + cameraOffset;
        float t = 1f - Mathf.Exp(-cameraFollowSmooth * Time.deltaTime);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, t);
        mainCamera.transform.rotation = fixedCameraRotation;
    }
}
