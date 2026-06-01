using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoystickTelemetryUI : MonoBehaviour
{
    [SerializeField] Image outerRing;
    [SerializeField] Image innerDot;
    [SerializeField] Image crosshairH;
    [SerializeField] Image crosshairV;
    [SerializeField] TMP_Text axisLabel;
    [SerializeField] TMP_Text axisValueText;
    [SerializeField] string widgetLabel = "INPUT";
    [SerializeField] float dotRadius = 28f;
    [SerializeField] float smoothing = 8f;
    [SerializeField] Color ringColor = new Color(0, 0.92f, 1, 0.55f);
    [SerializeField] Color dotColor = new Color(0, 0.92f, 1, 1);
    [SerializeField] Color crossColor = new Color(0, 0.92f, 1, 0.25f);

    Vector2 targetInput;
    Vector2 smoothedInput;
    RectTransform dotRect;

    void Awake()
    {
        if (innerDot) dotRect = innerDot.GetComponent<RectTransform>();
        if (outerRing) outerRing.color = ringColor;
        if (innerDot) innerDot.color = dotColor;
        if (crosshairH) crosshairH.color = crossColor;
        if (crosshairV) crosshairV.color = crossColor;
        if (axisLabel) axisLabel.text = widgetLabel;
    }

    void Update()
    {
        smoothedInput = Vector2.Lerp(smoothedInput, targetInput, Time.deltaTime * smoothing);
        ApplyDotPosition(smoothedInput);
        UpdateValueText(smoothedInput);
    }

    public void SetInput(Vector2 input) => targetInput = Vector2.ClampMagnitude(input, 1f);
    public void SetInput(float h, float v) => SetInput(new Vector2(h, v));

    void ApplyDotPosition(Vector2 input)
    {
        if (!dotRect) return;
        dotRect.anchoredPosition = input * dotRadius;
    }

    void UpdateValueText(Vector2 input)
    {
        if (!axisValueText) return;
        axisValueText.text = $"({input.x:+0.00;-0.00}, {input.y:+0.00;-0.00})";
    }
}