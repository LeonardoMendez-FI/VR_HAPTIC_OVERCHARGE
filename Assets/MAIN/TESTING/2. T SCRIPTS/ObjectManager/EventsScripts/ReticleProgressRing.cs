using UnityEngine;
using UnityEngine.UI;

public class ReticleProgressRing : MonoBehaviour
{
    public Image ringImage;
    public GazeEnergyDrainActor drainActor;
    public float graceDuration = 0.5f;
    public float smoothSpeed = 5f;

    private float targetFill;
    private float currentFill;

    void Update()
    {
        if (drainActor != null && drainActor.IsDraining)   // usamos la propiedad pública
        {
            targetFill = drainActor.currentEnergyNorm;
        }
        else
        {
            targetFill = 0f;
        }

        float speed = (targetFill < currentFill) ? 1f : (1f / graceDuration);
        currentFill = Mathf.MoveTowards(currentFill, targetFill, speed * Time.deltaTime);
        ringImage.fillAmount = currentFill;
    }
}