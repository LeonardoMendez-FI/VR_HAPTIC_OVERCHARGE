using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructureBarUI : MonoBehaviour
{
    [SerializeField] List<Image> segments = new List<Image>();
    [SerializeField] TMP_Text labelText;
    [SerializeField] TMP_Text valueText;
    [SerializeField] Color fullColor = new Color(0, 0.92f, 1, 1);
    [SerializeField] Color damagedColor = new Color(1, 0.55f, 0, 1);
    [SerializeField] Color criticalColor = new Color(1, 0.12f, 0, 1);
    [SerializeField] Color emptyColor = new Color(0.12f, 0.12f, 0.18f, 0.45f);
    [Range(0f, 1f)] [SerializeField] float damagedThreshold = 0.5f;
    [Range(0f, 1f)] [SerializeField] float criticalThreshold = 0.25f;
    [SerializeField] float flashDuration = 0.12f;
    [SerializeField] Color flashColor = Color.white;

    float currentValue = 1f;
    float flashTimer;

    void Awake()
    {
        if (labelText) labelText.text = "STRUCTURE";
        SetValue(1f);
    }

    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) RefreshSegments(currentValue);
        }
    }

    public void SetValue(float value)
    {
        currentValue = Mathf.Clamp01(value);
        RefreshSegments(currentValue);
        if (valueText) valueText.text = Mathf.RoundToInt(currentValue * 100f) + "%";
    }

    public void TriggerHitFlash()
    {
        flashTimer = flashDuration;
        foreach (var seg in segments)
            if (seg) seg.color = flashColor;
    }

    public void RegisterSegments(List<Image> newSegments) => segments = newSegments;

    void RefreshSegments(float value)
    {
        if (segments == null || segments.Count == 0) return;
        int total = segments.Count;
        int filledN = Mathf.RoundToInt(value * total);
        Color activeColor = value > damagedThreshold ? fullColor
                          : value > criticalThreshold ? damagedColor
                          : criticalColor;
        for (int i = 0; i < total; i++)
            if (segments[i]) segments[i].color = (i < filledN) ? activeColor : emptyColor;
    }
}