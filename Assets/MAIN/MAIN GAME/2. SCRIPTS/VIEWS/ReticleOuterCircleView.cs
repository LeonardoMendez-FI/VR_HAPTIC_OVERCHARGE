using UnityEngine;
using UnityEngine.UI;

public class ReticleOuterCircleView : ViewBase
{
    public Image outerCircle;
    public GazeManager gazeManager;
    public GazeEnergyDrainActor drainActor;

    [Header("Smooth Settings")]
    public float smoothSpeed = 10f;

    private EnergyManager targetEnergy;
    private IGazeTarget currentTarget;

    protected override void Subscribe()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged += OnTargetChanged;
            gazeManager.OnGazeTargetLost += OnTargetLost;
        }
    }

    protected override void Unsubscribe()
    {
        if (gazeManager != null)
        {
            gazeManager.OnGazeTargetChanged -= OnTargetChanged;
            gazeManager.OnGazeTargetLost -= OnTargetLost;
        }
    }

    private void Start()
    {
        if (outerCircle != null) outerCircle.fillAmount = 0f;
        OnTargetChanged(gazeManager != null ? gazeManager.CurrentTarget : null);
    }

    private void OnTargetChanged(IGazeTarget newTarget)
    {
        currentTarget = newTarget;
        if (newTarget == null)
        {
            targetEnergy = null;
            return;
        }

        var gazeTarget = newTarget as GazeTargetBehaviour;
        if (gazeTarget != null && gazeTarget.targetElectronicObject != null)
            targetEnergy = gazeTarget.targetElectronicObject.energyManager;
        else
            targetEnergy = null;
    }

    private void OnTargetLost()
    {
        // Limpiar referencias para ocultar el círculo
        currentTarget = null;
        targetEnergy = null;
    }

    private void Update()
    {
        if (outerCircle == null) return;

        float targetFill = 0f;

        if (currentTarget == null || targetEnergy == null)
        {
            targetFill = 0f;
        }
        else if (gazeManager.IsLocked)
        {
            if (drainActor != null)
                targetFill = drainActor.currentEnergyNorm;
            else
                targetFill = targetEnergy.normalized_local;
        }
        else
        {
            targetFill = targetEnergy.normalized_local * gazeManager.FocusProgress;
        }

        outerCircle.fillAmount = Mathf.Lerp(outerCircle.fillAmount, targetFill, Time.deltaTime * smoothSpeed);

        if (targetFill == 0f && outerCircle.fillAmount < 0.01f)
            outerCircle.fillAmount = 0f;
    }
}