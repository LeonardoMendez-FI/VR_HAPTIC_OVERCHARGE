using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenOverlayView : ViewBase
{
    public Image overlayImage;
    public Color baseColor = new Color(0f, 0.08f, 0.16f);
    public Color attackColor = new Color(0.8f, 0.1f, 0.1f);
    public float fadeDuration = 0.3f;
    public AttackSequenceActor attackSequenceActor;  // nuevo puente directo (o usar AttackEventBridge)

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (overlayImage != null)
            overlayImage.color = baseColor;
    }

    protected override void Subscribe()
    {
        if (attackSequenceActor != null)
        {
            attackSequenceActor.OnAttackStarted.AddListener(OnAttackStarted);
            attackSequenceActor.OnAttackEnded.AddListener(OnAttackEnded);
        }
    }

    protected override void Unsubscribe()
    {
        if (attackSequenceActor != null)
        {
            attackSequenceActor.OnAttackStarted.RemoveListener(OnAttackStarted);
            attackSequenceActor.OnAttackEnded.RemoveListener(OnAttackEnded);
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