using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenOverlayUIView : MonoBehaviour
{
    public Image overlayImage;
    public Color baseColor = new Color(0f, 0.08f, 0.16f);   // #001429
    public Color attackColor = new Color(0.8f, 0.1f, 0.1f);
    public float fadeDuration = 0.3f;
    public AttackManager attackManager;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (overlayImage != null)
            overlayImage.color = baseColor;
    }

    private void OnEnable()
    {
        if (attackManager != null)
        {
            attackManager.OnAttackStarted.AddListener(OnAttackStarted);
            attackManager.OnAttackEnded.AddListener(OnAttackEnded);
        }
    }

    private void OnDisable()
    {
        if (attackManager != null)
        {
            attackManager.OnAttackStarted.RemoveListener(OnAttackStarted);
            attackManager.OnAttackEnded.RemoveListener(OnAttackEnded);
        }
    }

    private void OnAttackStarted()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(attackColor));
    }

    private void OnAttackEnded()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(baseColor));
    }

    private IEnumerator FadeTo(Color target)
    {
        Color start = overlayImage.color;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            overlayImage.color = Color.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }
        overlayImage.color = target;
    }
}