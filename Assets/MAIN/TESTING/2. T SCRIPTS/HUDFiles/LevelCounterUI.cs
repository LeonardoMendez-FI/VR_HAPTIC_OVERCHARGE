// ============================================================
//  LevelTitleUI.cs
//  Sci-Fi Robot HUD System — Level Title Display
//  ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoboticHUD.Elements
{
    /// <summary>
    /// TOP CENTER title card. Animates in with a scan-reveal
    /// on level load, then fades to a subtle persistent state.
    /// </summary>
    public class LevelTitleUI : MonoBehaviour
    {
        [Header("─── Components ─────────────────────────────")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text subtitleText;
        [SerializeField] Image    accentLineLeft;
        [SerializeField] Image    accentLineRight;

        [Header("─── Display Settings ──────────────────────")]
        [SerializeField] float holdDuration  = 3.5f;   // seconds title stays full-opacity
        [SerializeField] float fadeDuration  = 1.2f;   // fade to persistent alpha
        [SerializeField] float persistAlpha  = 0.45f;  // opacity when not featured

        [Header("─── Colors ──────────────────────────────────")]
        [SerializeField] Color titleColor    = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        [SerializeField] Color subtitleColor = new Color(0.0f, 0.92f, 1.0f, 1.0f);
        [SerializeField] Color accentColor   = new Color(0.0f, 0.92f, 1.0f, 0.8f);

        // ── State ────────────────────────────────────────────
        CanvasGroup canvasGroup;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (accentLineLeft  != null) accentLineLeft.color  = accentColor;
            if (accentLineRight != null) accentLineRight.color = accentColor;

            canvasGroup.alpha = 0f;
        }

        // ── Public API ───────────────────────────────────────

        /// <summary>
        /// Display a new level title.
        /// titleLine: "LEVEL 2 — ROBOTICS LABORATORY"
        /// Optional subtitle: "SECTOR 04 / RESTRICTED ACCESS"
        /// </summary>
        public void SetTitle(string titleLine, string subtitle = "")
        {
            if (titleText    != null) { titleText.text = titleLine; titleText.color = titleColor; }
            if (subtitleText != null) { subtitleText.text = subtitle; subtitleText.color = subtitleColor; }

            StopAllCoroutines();
            StartCoroutine(RevealRoutine());
        }

        // ── Private ──────────────────────────────────────────

        IEnumerator RevealRoutine()
        {
            // Scan-reveal: quickly fade in
            float t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.4f);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(holdDuration);

            // Fade to persistent alpha
            float startAlpha = canvasGroup.alpha;
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, persistAlpha, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = persistAlpha;
        }
    }
}


// ============================================================
//  CountersUI.cs
//  Sci-Fi Robot HUD System — Kill / Objective Counters
//  ============================================================

namespace RoboticHUD.Elements
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// TOP RIGHT compact vertical counters.
    /// Two rows: Robot Eliminations + Remaining Objectives.
    /// </summary>
    public class CountersUI : MonoBehaviour
    {
        [Header("─── Eliminations ───────────────────────────")]
        [SerializeField] TMP_Text eliminationLabel;
        [SerializeField] TMP_Text eliminationCount;
        [SerializeField] Image    eliminationIcon;

        [Header("─── Objectives ─────────────────────────────")]
        [SerializeField] TMP_Text objectiveLabel;
        [SerializeField] TMP_Text objectiveCount;
        [SerializeField] Image    objectiveIcon;

        [Header("─── Colors ──────────────────────────────────")]
        [SerializeField] Color labelColor  = new Color(0.0f, 0.92f, 1.0f, 0.75f);
        [SerializeField] Color valueColor  = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        [SerializeField] Color pulseColor  = new Color(1.0f, 0.3f, 0.0f, 1.0f);   // flash on change

        int currentEliminations;
        int currentObjectives;

        void Awake()
        {
            if (eliminationLabel != null) { eliminationLabel.text  = "ELIMINATED"; eliminationLabel.color = labelColor; }
            if (objectiveLabel   != null) { objectiveLabel.text    = "REMAINING";  objectiveLabel.color   = labelColor; }
            SetEliminations(0);
            SetObjectives(0);
        }

        // ── Public API ───────────────────────────────────────

        public void SetEliminations(int count)
        {
            currentEliminations = count;
            if (eliminationCount != null)
            {
                eliminationCount.text  = count.ToString("D3");
                eliminationCount.color = valueColor;
                StartCoroutine(PulseText(eliminationCount));
            }
        }

        public void SetObjectives(int remaining)
        {
            currentObjectives = remaining;
            if (objectiveCount != null)
            {
                objectiveCount.text  = remaining.ToString("D3");
                objectiveCount.color = remaining == 0 ? pulseColor : valueColor;
            }
        }

        System.Collections.IEnumerator PulseText(TMP_Text t)
        {
            Color original = t.color;
            t.color = pulseColor;
            yield return new WaitForSeconds(0.25f);
            t.color = original;
        }
    }
}
