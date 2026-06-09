using System;
using UnityEngine;

public class GazeManager : ManagerScript
{
    [Header("Gaze Detection")]
    public Transform gazeOrigin;
    public LayerMask gazeLayerMask;
    public float maxGazeDistance = 20f;

    [Header("Focus Timing")]
    public float focusThreshold = 1.5f;
    [Range(0.1f, 5f)] public float decayRate = 0.6f;
    public bool resetProgressOnTargetChange = true;

    [Header("Sticky Stabilization")]
    public StickyTargetLogic stickyTargetLogic;

    [Header("Lock Grace")]
    [Tooltip("Tiempo en segundos que el lock se mantiene tras perder el objetivo.")]
    public float lockGraceDuration = 3f;

    public IGazeTarget CurrentTarget { get; private set; }
    public float FocusProgress { get; private set; }
    public bool IsLocked { get; private set; }

    public event Action<IGazeTarget> OnGazeTargetChanged;
    public event Action<IGazeTarget> OnGazeTargetFocused;
    public event Action OnGazeTargetLost;
    public event Action<float> OnGazeFocusProgress;

    private float prevFocusProgress;
    private IGazeTarget lockedTarget;       // objetivo al que se hizo lock
    private float lockGraceTimer;           // cuenta atrás para perder el lock

    private void Awake()
    {
        if (gazeOrigin == null)
            gazeOrigin = Camera.main?.transform;
    }

    public override void Update()
    {
        IGazeTarget detected = DetectTarget();

        IGazeTarget stabilizedTarget = stickyTargetLogic != null
            ? stickyTargetLogic.Stabilize(detected)
            : detected;

        HandleTargetTransition(stabilizedTarget);
        UpdateFocusProgress();
        UpdateLockGrace();

        if (!Mathf.Approximately(prevFocusProgress, FocusProgress))
        {
            OnGazeFocusProgress?.Invoke(FocusProgress);
            prevFocusProgress = FocusProgress;
        }

        foreach (ActorBase actor in actors)
        {
            if (actor != null)
                actor.Solve();
        }
    }

    private void HandleTargetTransition(IGazeTarget newTarget)
    {
        // Si estamos en gracia y reaparece el mismo objetivo lockeado
        if (IsLocked && lockedTarget != null && newTarget == lockedTarget)
        {
            // Cancelar gracia y restaurar el objetivo sin perder el lock
            lockGraceTimer = 0f;
            if (CurrentTarget == null)
            {
                CurrentTarget = newTarget;
                // No llamamos a OnGazeEnter porque ya estaba lockeado, solo notificamos el cambio
                OnGazeTargetChanged?.Invoke(CurrentTarget);
            }
            return;
        }

        // Si hay un nuevo target diferente y estamos lockeados, forzar pérdida del lock
        if (IsLocked && lockedTarget != null && newTarget != null && newTarget != lockedTarget)
        {
            ForceReleaseLock();
        }

        // Flujo normal de cambio de objetivo
        if (newTarget == CurrentTarget) return;

        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeExit();
            OnGazeTargetLost?.Invoke(); // se invoca inmediatamente al perder el objetivo
        }

        CurrentTarget = newTarget;

        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeEnter();
            OnGazeTargetChanged?.Invoke(CurrentTarget);
        }
        else if (IsLocked)
        {
            // Se perdió el objetivo pero estamos lockeados → iniciar gracia
            lockGraceTimer = lockGraceDuration;
            OnGazeTargetLost?.Invoke(); // notificar que se perdió visualmente
        }
    }

    private void UpdateFocusProgress()
    {
        if (CurrentTarget != null)
        {
            FocusProgress = Mathf.Clamp01(FocusProgress + Time.deltaTime / focusThreshold);

            if (!IsLocked && FocusProgress >= 1f)
            {
                IsLocked = true;
                lockedTarget = CurrentTarget;
                CurrentTarget.OnGazeFocused();
                OnGazeTargetFocused?.Invoke(CurrentTarget);
            }
        }
        else if (!IsLocked)
        {
            // Sin lock y sin objetivo, decaer
            FocusProgress = Mathf.Clamp01(FocusProgress - Time.deltaTime * decayRate);
        }
    }

    private void UpdateLockGrace()
    {
        if (lockGraceTimer > 0f)
        {
            lockGraceTimer -= Time.deltaTime;
            if (lockGraceTimer <= 0f)
            {
                // La gracia expiró → perder el lock definitivamente
                ForceReleaseLock();
            }
        }
    }

    private void ForceReleaseLock()
    {
        if (!IsLocked) return;

        IsLocked = false;
        FocusProgress = 0f;
        lockGraceTimer = 0f;
        lockedTarget = null;

        if (CurrentTarget != null)
        {
            CurrentTarget.OnGazeExit();
            CurrentTarget = null;
        }

        OnGazeTargetLost?.Invoke();
        OnGazeTargetChanged?.Invoke(null);
    }

    private IGazeTarget DetectTarget()
    {
        if (gazeOrigin == null) return null;

        Ray ray = new Ray(gazeOrigin.position, gazeOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxGazeDistance, gazeLayerMask))
        {
            var target = hit.collider.GetComponent<IGazeTarget>();
            if (target != null && ((1 << hit.collider.gameObject.layer) & target.GazeLayerMask) != 0)
                return target;
        }
        return null;
    }

    public void ClearTarget()
    {
        HandleTargetTransition(null);
        stickyTargetLogic?.ClearSticky();
    }

    public void ForceReleaseTarget()
    {
        ForceReleaseLock(); // ya se encarga de limpiar todo
        stickyTargetLogic?.ClearSticky();
    }
}