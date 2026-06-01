using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovementModeUI : MonoBehaviour
{
    [SerializeField] Image modeIcon;
    [SerializeField] TMP_Text modeLabel;
    [SerializeField] Image backgroundPanel;
    [SerializeField] Sprite flightIcon;
    [SerializeField] Sprite walkIcon;
    [SerializeField] Color flightColor = new Color(0, 0.92f, 1, 1);
    [SerializeField] Color walkColor = new Color(0.75f, 0.75f, 1, 1);
    [SerializeField] Color panelColor = new Color(0, 0.08f, 0.18f, 0.65f);
    [SerializeField] float flickerDuration = 0.25f;
    [SerializeField] int flickerCount = 4;

    bool isFlightMode;
    bool isTransitioning;

    void Awake()
    {
        if (backgroundPanel) backgroundPanel.color = panelColor;
        ApplyMode(false, true);
    }

    public void SetMode(bool isFlying)
    {
        if (isTransitioning) StopAllCoroutines();
        StartCoroutine(TransitionRoutine(isFlying));
    }

    IEnumerator TransitionRoutine(bool newMode)
    {
        isTransitioning = true;
        float interval = flickerDuration / (flickerCount * 2f);
        for (int i = 0; i < flickerCount; i++)
        {
            SetVisible(false); yield return new WaitForSeconds(interval);
            SetVisible(true); yield return new WaitForSeconds(interval);
        }
        ApplyMode(newMode);
        isTransitioning = false;
    }

    void ApplyMode(bool flying, bool instant = false)
    {
        isFlightMode = flying;
        string label = flying ? "FLIGHT MODE" : "WALK MODE";
        Color color = flying ? flightColor : walkColor;
        Sprite icon = flying ? flightIcon : walkIcon;
        if (modeLabel) { modeLabel.text = label; modeLabel.color = color; }
        if (modeIcon) { if (icon) modeIcon.sprite = icon; modeIcon.color = color; }
    }

    void SetVisible(bool visible)
    {
        float a = visible ? 1f : 0f;
        if (modeLabel) { var c = modeLabel.color; c.a = a; modeLabel.color = c; }
        if (modeIcon) { var c = modeIcon.color; c.a = a; modeIcon.color = c; }
    }
}