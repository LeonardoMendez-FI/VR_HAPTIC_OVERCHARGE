using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTitleUI : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text subtitleText;
    [SerializeField] Image accentLineLeft;
    [SerializeField] Image accentLineRight;
    [SerializeField] float holdDuration = 3.5f;
    [SerializeField] float fadeDuration = 1.2f;
    [SerializeField] float persistAlpha = 0.45f;
    [SerializeField] Color titleColor = Color.white;
    [SerializeField] Color subtitleColor = new Color(0, 0.92f, 1, 1);
    [SerializeField] Color accentColor = new Color(0, 0.92f, 1, 0.8f);

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (accentLineLeft) accentLineLeft.color = accentColor;
        if (accentLineRight) accentLineRight.color = accentColor;
        canvasGroup.alpha = 0f;
    }

    public void SetTitle(string titleLine, string subtitle = "")
    {
        if (titleText) { titleText.text = titleLine; titleText.color = titleColor; }
        if (subtitleText) { subtitleText.text = subtitle; subtitleText.color = subtitleColor; }
        StopAllCoroutines();
        StartCoroutine(RevealRoutine());
    }

    IEnumerator RevealRoutine()
    {
        float t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.4f); yield return null; }
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(holdDuration);
        float startAlpha = canvasGroup.alpha;
        t = 0f;
        while (t < fadeDuration) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(startAlpha, persistAlpha, t / fadeDuration); yield return null; }
        canvasGroup.alpha = persistAlpha;
    }
}