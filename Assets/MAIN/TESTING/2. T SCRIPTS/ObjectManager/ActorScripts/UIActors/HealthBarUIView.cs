using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIView : MonoBehaviour
{
    [Header("References")]
    public Image healthBarImage;
    public StructManager structManager;

    [Header("Colors")]
    public Color fullColor = new Color(0f, 1f, 0.58f);   // #00FF95
    public Color halfColor = Color.yellow;
    public Color lowColor = Color.red;

    private void OnEnable()
    {
        if (structManager != null)
            structManager.OnStructureChanged.AddListener(UpdateBar);
    }

    private void OnDisable()
    {
        if (structManager != null)
            structManager.OnStructureChanged.RemoveListener(UpdateBar);
    }

    private void UpdateBar(float normalized)
    {
        if (healthBarImage == null) return;
        healthBarImage.fillAmount = normalized;

        // Color gradient: green -> yellow -> red
        Color color;
        if (normalized >= 0.5f)
            color = Color.Lerp(halfColor, fullColor, (normalized - 0.5f) * 2f);
        else
            color = Color.Lerp(lowColor, halfColor, normalized * 2f);
        healthBarImage.color = color;
    }
}