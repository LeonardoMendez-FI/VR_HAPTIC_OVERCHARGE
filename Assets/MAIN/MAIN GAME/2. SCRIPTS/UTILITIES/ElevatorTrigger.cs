using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [Header("Level Service")]
    public LevelService levelService;

    [Header("Door Visual (Mesh to hide when opened)")]
    public MeshRenderer doorVisual;

    [Header("Door Collider (BoxCollider recomendado)")]
    public Collider doorCollider;      // ← asígnale tu BoxCollider desde el Inspector

    private enum ElevatorState { WaitingForEntry, LevelActive, WaitingForExit }
    private ElevatorState state = ElevatorState.WaitingForEntry;
    private Rigidbody doorRigidbody;

    void Awake()
    {
        // Si no se asignó en el Inspector, buscarlo en hijos
        if (doorCollider == null)
            doorCollider = GetComponentInChildren<Collider>();

        if (doorCollider == null)
        {
            Debug.LogError("[ElevatorTrigger] No se encontró ningún Collider. Asigna uno en el Inspector.");
            return;
        }

        // Si es MeshCollider, forzar convexo ANTES de tocar isTrigger
        if (doorCollider is MeshCollider mc)
            mc.convex = true;

        // Activar trigger
        doorCollider.isTrigger = true;

        // Rigidbody necesario para eventos de trigger
        doorRigidbody = GetComponent<Rigidbody>();
        if (doorRigidbody == null)
        {
            doorRigidbody = gameObject.AddComponent<Rigidbody>();
            doorRigidbody.isKinematic = true;
            doorRigidbody.useGravity = false;
        }
    }

    void Start()
    {
        if (levelService == null)
            Debug.LogError("[ElevatorTrigger] LevelService no asignado.");
        state = ElevatorState.WaitingForEntry;
    }

    void OnDestroy()
    {
        if (levelService != null)
            levelService.OnAllMachinesDestroyed.RemoveListener(OnAllMachinesDestroyed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (levelService == null) return;
        if (levelService.playerTarget == null)
        {
            var player = FindFirstObjectByType<PlayerRobot>();
            if (player != null) levelService.playerTarget = player.transform;
        }
        if (levelService.playerTarget == null) return;
        if (other.transform.root != levelService.playerTarget.root) return;

        if (state == ElevatorState.WaitingForExit)
            levelService.LoadNextLevel();
    }

    void OnTriggerExit(Collider other)
    {
        if (levelService == null || levelService.playerTarget == null) return;
        if (other.transform.root != levelService.playerTarget.root) return;

        if (state == ElevatorState.WaitingForEntry)
        {
            state = ElevatorState.LevelActive;
            levelService.StartLevel();
            levelService.OnAllMachinesDestroyed.AddListener(OnAllMachinesDestroyed);
            doorCollider.isTrigger = false;   // sellar
        }
    }

    void OnAllMachinesDestroyed()
    {
        doorCollider.isTrigger = true;        // reabrir
        if (doorVisual != null)
            doorVisual.enabled = false;       // ocultar malla

        if (levelService != null)
            levelService.OnAllMachinesDestroyed.RemoveListener(OnAllMachinesDestroyed);

        state = ElevatorState.WaitingForExit;
    }
}