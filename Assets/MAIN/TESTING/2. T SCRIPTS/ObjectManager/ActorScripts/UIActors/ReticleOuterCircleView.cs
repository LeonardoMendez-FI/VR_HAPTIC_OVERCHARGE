using UnityEngine;
using UnityEngine.UI;

public class ReticleOuterCircleView : MonoBehaviour
{
    [Header("References")]
    public Image outerCircle;             // Image con Fill Method = Radial 360
    public GazeManager gazeManager;
    public GazeEnergyDrainActor drainActor;

    [Header("Settings")]
    public float graceDuration = 0.5f;    // duración de la interpolación inicial

    private EnergyManager targetEnergy;
    private float graceTimer;
    private bool isApproaching;           // true durante la fase de gracia (antes del lock)
    private IGazeTarget currentTarget;

    private void OnEnable()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged += OnTargetChanged;
            gazeManager.OnGazeTargetFocused += OnTargetFocused;
        }
    }

    private void OnDisable()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged -= OnTargetChanged;
            gazeManager.OnGazeTargetFocused -= OnTargetFocused;
        }
    }

    private void Start()
    {
        OnTargetChanged(gazeManager != null ? gazeManager.CurrentTarget : null);
    }

    private void OnTargetChanged(IGazeTarget newTarget)
    {
        currentTarget = newTarget;
        if (newTarget == null)
        {
            targetEnergy = null;
            isApproaching = false;
            outerCircle.fillAmount = 0f;
            return;
        }

        // Cachear EnergyManager del nuevo objetivo
        var go = (newTarget as MonoBehaviour)?.gameObject;
        var eo = go?.GetComponent<ElectronicObject>();
        targetEnergy = eo?.energyManager;

        // Empezar fase de gracia solo si aún no estamos drenando
        if (drainActor == null || !drainActor.IsDraining)
        {
            isApproaching = true;
            graceTimer = 0f;
        }
    }

    private void OnTargetFocused(IGazeTarget focusedTarget)
    {
        // Cuando se alcanza el lock, terminamos la fase de gracia
        isApproaching = false;
    }

    private void Update()
    {
        if (outerCircle == null || gazeManager == null) return;

        // Si no hay objetivo, fill 0
        if (currentTarget == null || targetEnergy == null)
        {
            outerCircle.fillAmount = 0f;
            return;
        }

        float targetFill = 0f;

        // Si estamos drenando, seguimos la energía drenada directamente
        if (drainActor != null && drainActor.IsDraining)
        {
            isApproaching = false;
            targetFill = drainActor.currentEnergyNorm;
        }
        // Si estamos en fase de gracia (antes del lock)
        else if (isApproaching)
        {
            float energyNorm = targetEnergy.normalized_local;
            graceTimer += Time.deltaTime;
            if (graceTimer >= graceDuration)
            {
                targetFill = energyNorm;
                isApproaching = false;   // terminó la gracia
            }
            else
            {
                float t = graceTimer / graceDuration;
                targetFill = Mathf.Lerp(0f, energyNorm, t);
            }
        }
        // En cualquier otro caso (lock pero aún no drenando, o mientras se enfoca)
        else
        {
            targetFill = targetEnergy.normalized_local;
        }

        // Aplicar suavizado
        outerCircle.fillAmount = Mathf.Lerp(outerCircle.fillAmount, targetFill, Time.deltaTime * 10f);
    }
}