using UnityEngine;
using UnityEngine.UI;

public class ReticleOuterCircleView : ViewBase
{
    public Image outerCircle;
    public GazeManager gazeManager;
    public GazeEnergyDrainActor drainActor;
    public float graceDuration = 0.5f;

    private EnergyManager targetEnergy;
    private float graceTimer;
    private bool isApproaching;
    private IGazeTarget currentTarget;

    protected override void Subscribe()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged += OnTargetChanged;
            gazeManager.OnGazeTargetFocused += OnTargetFocused;
        }
    }

    protected override void Unsubscribe()
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

        var go = (newTarget as MonoBehaviour)?.gameObject;
        var eo = go?.GetComponent<ElectronicObject>();
        targetEnergy = eo?.energyManager;

        if (drainActor == null || !drainActor.IsDraining)
        {
            isApproaching = true;
            graceTimer = 0f;
        }
    }

    private void OnTargetFocused(IGazeTarget focusedTarget)
    {
        isApproaching = false;
    }

    private void Update()
    {
        if (outerCircle == null || gazeManager == null || currentTarget == null || targetEnergy == null)
        {
            if (outerCircle != null && !Mathf.Approximately(outerCircle.fillAmount, 0f))
                outerCircle.fillAmount = 0f;
            return;
        }

        float targetFill = 0f;

        if (drainActor != null && drainActor.IsDraining)
        {
            isApproaching = false;
            targetFill = drainActor.currentEnergyNorm;
        }
        else if (isApproaching)
        {
            float energyNorm = targetEnergy.normalized_local;
            graceTimer += Time.deltaTime;
            if (graceTimer >= graceDuration)
            {
                targetFill = energyNorm;
                isApproaching = false;
            }
            else
            {
                float t = graceTimer / graceDuration;
                targetFill = Mathf.Lerp(0f, energyNorm, t);
            }
        }
        else
        {
            targetFill = targetEnergy.normalized_local;
        }

        if (Mathf.Approximately(outerCircle.fillAmount, targetFill)) return;
        outerCircle.fillAmount = Mathf.Lerp(outerCircle.fillAmount, targetFill, Time.deltaTime * 10f);
    }
}