/// <summary>
/// Author: Andre
/// StudentNo: 10273383D
/// Purpose:
/// Spawns the return owl after the player has collected all required evidence
/// in the Virtual World scene.
/// </summary>
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the return owl used at the end of the Virtual World investigation.
///
/// This component has two responsibilities:
/// <list type="number">
/// <item>It listens for the evidence system to report that all required evidence has been collected.</item>
/// <item>It creates one owl near the player and moves that owl using a NavMeshAgent.</item>
/// </list>
///
/// The owl prefab is responsible for its own visual appearance, collider, and interaction components.
/// This component is responsible for deciding when the owl exists and where it moves.
/// </summary>
public class EvidenceReturnOwlSpawner : MonoBehaviour
{
    [Header("Owl")]
    /// <summary>
    /// Prefab that will be created when the evidence requirement is complete.
    /// The prefab should normally contain a NavMeshAgent, a collider, and the
    /// TeleporterScript that sends the player back to TheOffice.
    /// </summary>
    [SerializeField] private GameObject owlPrefab;

    /// <summary>
    /// Optional location used only if the player cannot be found.
    /// This is useful as a safe fallback when testing the scene or when the
    /// player object is created after this component.
    /// </summary>
    [SerializeField] private Transform fallbackSpawnPoint;

    /// <summary>
    /// How far behind the player the owl should initially appear.
    /// A larger value makes the owl appear farther away from the player.
    /// </summary>
    [SerializeField] private float distanceBehindPlayer = 2.5f;

    /// <summary>
    /// Maximum distance used when searching for a valid NavMesh position near
    /// the desired spawn location. The owl must be placed on the NavMesh so
    /// its NavMeshAgent can move immediately after spawning.
    /// </summary>
    [SerializeField] private float navMeshSampleDistance = 3f;

    [Header("Owl Movement")]
    /// <summary>
    /// Distance at which the owl stops following the player.
    /// This value is copied to the spawned NavMeshAgent's stoppingDistance.
    /// </summary>
    [SerializeField] private float owlStopDistance = 2f;

    /// <summary>
    /// Time in seconds between destination updates. The owl does not need a
    /// new path every frame, so a small delay reduces unnecessary pathfinding.
    /// </summary>
    [SerializeField] private float destinationUpdateRate = 0.25f;

    /// <summary>Reference to the owl instance created by this component.</summary>
    private GameObject _owlInstance;

    /// <summary>NavMeshAgent found on the spawned owl prefab.</summary>
    private NavMeshAgent _owlAgent;

    /// <summary>Persistent manager that stores the evidence collected by the player.</summary>
    private GameFlowManager _flowManager;

    /// <summary>Player whose position is used for the owl spawn and follow target.</summary>
    private Player _player;

    /// <summary>Time at which the owl is allowed to request its next path.</summary>
    private float _nextDestinationUpdate;

    private bool _isSubscribedToEvidence;

    /// <summary>
    /// Gets references that are likely to be available before the scene starts.
    ///
    /// GameFlowManager is persistent between scenes, while Player belongs to
    /// the active gameplay scene. The extra checks in Start are intentional:
    /// they handle cases where Unity has not finished creating one of these
    /// objects during Awake.
    /// </summary>
    private void Awake()
    {
        _flowManager = GameFlowManager.Instance;
        _player = FindFirstObjectByType<Player>();
    }

    /// <summary>
    /// Finishes setup after all scene objects have been initialized.
    ///
    /// The EvidenceChanged event is important because the final piece of
    /// evidence may be collected after this object starts. The immediate
    /// HasRequiredEvidence check handles the opposite case: the player may
    /// return to this scene with the requirement already completed.
    /// </summary>
    private void Start()
    {
        FindSceneReferences();
        CheckEvidenceRequirement();
    }

    /// <summary>
    /// Removes the event subscription when the spawner is disabled.
    ///
    /// Event subscriptions should be removed when they are no longer needed;
    /// otherwise the manager could keep calling this object after the scene
    /// has been unloaded or the object has been disabled.
    /// </summary>
    private void OnDisable()
    {
        if (_flowManager != null && _isSubscribedToEvidence)
        {
            _flowManager.EvidenceChanged -= HandleEvidenceChanged;
            _isSubscribedToEvidence = false;
        }
    }

    private void FindSceneReferences()
    {
        if (_flowManager == null)
        {
            _flowManager = GameFlowManager.Instance;
        }

        if (_player == null)
        {
            _player = FindFirstObjectByType<Player>();
        }

        if (_flowManager != null && !_isSubscribedToEvidence)
        {
            _flowManager.EvidenceChanged += HandleEvidenceChanged;
            _isSubscribedToEvidence = true;
        }
    }

    private void CheckEvidenceRequirement()
    {
        if (_flowManager != null && _flowManager.HasRequiredEvidence)
        {
            SpawnOwl();
        }
    }

    /// <summary>
    /// Responds to the evidence manager changing its progress.
    ///
    /// EvidenceChanged can be raised for any newly collected evidence, so the
    /// HasRequiredEvidence check prevents the owl from appearing too early.
    /// SpawnOwl also checks whether an instance already exists, which means
    /// collecting more evidence cannot create duplicate owls.
    /// </summary>
    private void HandleEvidenceChanged()
    {
        if (_flowManager != null && _flowManager.HasRequiredEvidence)
        {
            SpawnOwl();
        }
    }

    /// <summary>
    /// Periodically updates the owl's NavMesh destination.
    ///
    /// The owl follows the player's current position rather than a fixed point.
    /// The NavMeshAgent calculates the walkable path. If the owl is not on a
    /// NavMesh, no destination is requested because SetDestination would not
    /// be able to move it correctly.
    /// </summary>
    private void Update()
    {
        if (_owlInstance == null)
        {
            FindSceneReferences();
            CheckEvidenceRequirement();
        }

        if (_owlInstance == null || _owlAgent == null || _player == null)
        {
            return;
        }

        if (Time.time < _nextDestinationUpdate || !_owlAgent.isOnNavMesh)
        {
            return;
        }

        _nextDestinationUpdate = Time.time + destinationUpdateRate;
        _owlAgent.isStopped = false;
        _owlAgent.SetDestination(_player.transform.position);
    }

    /// <summary>
    /// Creates and configures the owl instance.
    ///
    /// The preferred position is behind the player, based on the player's
    /// forward direction. NavMesh.SamplePosition moves that desired location
    /// onto nearby walkable geometry. If the player is unavailable, the method
    /// uses the optional fallback point, then finally uses this object's own
    /// transform as a last-resort testing position.
    /// </summary>
    private void SpawnOwl()
    {
        if (_owlInstance != null || owlPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (_player != null)
        {
            Vector3 behindDirection = -_player.transform.forward;
            behindDirection.y = 0f;

            if (behindDirection.sqrMagnitude < 0.01f)
            {
                behindDirection = Vector3.back;
            }

            Vector3 desiredPosition = _player.transform.position + behindDirection.normalized * distanceBehindPlayer;
            spawnPosition = desiredPosition;
            spawnRotation = Quaternion.LookRotation(-behindDirection, Vector3.up);

            if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = navHit.position;
            }
        }
        else if (fallbackSpawnPoint != null)
        {
            spawnPosition = fallbackSpawnPoint.position;
            spawnRotation = fallbackSpawnPoint.rotation;
        }
        else
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        _owlInstance = Instantiate(owlPrefab, spawnPosition, spawnRotation);
        _owlAgent = _owlInstance.GetComponent<NavMeshAgent>();

        if (_owlAgent != null)
        {
            _owlAgent.stoppingDistance = owlStopDistance;
            _owlAgent.autoBraking = true;
        }
    }
}