using UnityEngine;
using UnityEngine.UI;

public class VisorFrameUI : MonoBehaviour
{
    [SerializeField] Image frameImage;
    [SerializeField] Color normalColor = new Color(0, 0.9f, 1, 0.18f);
    [SerializeField] Color criticalColor = new Color(1, 0.2f, 0, 0.35f);
    [SerializeField] bool enablePulse = true;
    [SerializeField] float pulseSpeed = 0.6f;
    [SerializeField] float pulseAmplitude = 0.06f;
    [SerializeField] bool enableScanlines = true;
    [SerializeField] float scanlineSpeed = 0.4f;

    bool isCritical;
    float baseAlpha;
    float scanOffset;
    static readonly int k_ScanlineOffset = Shader.PropertyToID("_ScanlineOffset");

    void Awake()
    {
        if (!frameImage) frameImage = GetComponent<Image>();
        baseAlpha = normalColor.a;
    }

    void Update()
    {
        AnimatePulse();
        AnimateScanlines();
    }

    public void SetCriticalWarning(bool critical)
    {
        isCritical = critical;
        if (!critical) { var c = normalColor; c.a = baseAlpha; frameImage.color = c; }
    }

    void AnimatePulse()
    {
        if (!enablePulse) return;
        float t = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);
        Color target = isCritical ? criticalColor : normalColor;
        target.a = Mathf.Clamp01(baseAlpha + t * pulseAmplitude);
        frameImage.color = target;
    }

    void AnimateScanlines()
    {
        if (!enableScanlines || frameImage.material == null) return;
        scanOffset += Time.deltaTime * scanlineSpeed;
        if (scanOffset > 1f) scanOffset -= 1f;
        frameImage.material.SetFloat(k_ScanlineOffset, scanOffset);
    }
}