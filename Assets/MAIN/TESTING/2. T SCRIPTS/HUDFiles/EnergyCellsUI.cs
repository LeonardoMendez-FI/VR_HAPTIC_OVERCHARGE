using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyCellsUI : MonoBehaviour
{
    [SerializeField] List<Image> cells = new List<Image>();
    [SerializeField] TMP_Text labelText;
    [SerializeField] TMP_Text percentText;
    [SerializeField] Color fullColor = new Color(0, 0.92f, 1, 1);
    [SerializeField] Color depletedColor = new Color(0.05f, 0.15f, 0.25f, 0.5f);
    [SerializeField] Color lowColor = new Color(1, 0.4f, 0, 1);
    [SerializeField] Color rechargeColor = new Color(0.6f, 1, 1, 1);
    [SerializeField] bool enableGlow = true;
    [SerializeField] float glowSpeed = 1.4f;
    [SerializeField] float glowAmplitude = 0.18f;
    [Range(0f, 1f)] [SerializeField] float lowThreshold = 0.3f;

    float currentEnergy = 1f;
    float lastEnergy = 1f;
    bool isRecharging;
    float glowPhase;

    void Awake()
    {
        if (labelText) labelText.text = "ENERGY";
        SetValue(1f);
    }

    void Update()
    {
        if (enableGlow) AnimateGlow();
    }

    public void SetValue(float value)
    {
        lastEnergy = currentEnergy;
        currentEnergy = Mathf.Clamp01(value);
        isRecharging = currentEnergy > lastEnergy;
        RefreshCells();
        if (percentText) percentText.text = Mathf.RoundToInt(currentEnergy * 100f) + "%";
    }

    public void RegisterCells(List<Image> newCells) => cells = newCells;

    void RefreshCells()
    {
        if (cells == null || cells.Count == 0) return;
        int total = cells.Count;
        int filledN = Mathf.RoundToInt(currentEnergy * total);
        bool isLow = currentEnergy <= lowThreshold;
        Color activeColor = isLow ? lowColor : fullColor;
        for (int i = 0; i < total; i++)
            if (cells[i]) cells[i].color = (i < filledN) ? activeColor : depletedColor;
    }

    void AnimateGlow()
    {
        if (cells == null || cells.Count == 0) return;
        glowPhase += Time.deltaTime * glowSpeed;
        int total = cells.Count;
        int filledN = Mathf.RoundToInt(currentEnergy * total);
        bool isLow = currentEnergy <= lowThreshold;
        for (int i = 0; i < filledN; i++)
        {
            if (!cells[i]) continue;
            float phase = glowPhase - i * 0.15f;
            float alpha = 1f + Mathf.Sin(phase * Mathf.PI * 2f) * glowAmplitude;
            Color c = isLow ? lowColor : (isRecharging ? rechargeColor : fullColor);
            c.a = Mathf.Clamp01(alpha);
            cells[i].color = c;
        }
    }
}