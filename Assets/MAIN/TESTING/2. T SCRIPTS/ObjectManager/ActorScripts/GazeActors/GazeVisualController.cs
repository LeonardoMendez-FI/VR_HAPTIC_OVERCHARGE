using System.Collections;
using UnityEngine;

/// <summary>
/// Visual controller owned by each scannable target.
/// Responds to state callbacks from GazeTargetBehaviour and drives:
///   - Outline renderers (holographic edge highlight)
///   - Shader-driven progress rings (fill 0..1)
///   - Lock VFX (particle systems, effect objects)
///   - Smooth fade-out on gaze loss
///
/// SHADER CONVENTIONS:
///   Outline shader should expose:
///     _OutlineColor (Color)   — RGB+A tint of the outline
///     _OutlineIntensity (Float, 0..1) — master intensity/alpha
///
///   Progress shader should expose:
///     _ScanProgress (Float, 0..1) — ring fill amount
///     _ProgressColor (Color)      — ring color
///
///   These are convention names — override them in the Inspector if your
///   shaders use different property names.
///
/// SETUP:
///   1. Create a separate "outline mesh" child (same mesh, front-face culled)
///      with an outline/holographic shader, assign its Renderer to outlineRenderers.
///   2. Create a "progress ring" mesh (ring geometry or quad with ring shader),
///      assign its Renderer to progressRenderers.
///   3. Create any lock VFX as child GameObjects (start disabled), assign to lockVFXObjects.
///   4. Tweak colors and timing in the Inspector.
/// </summary>
[DisallowMultipleComponent]
public class GazeVisualController : MonoBehaviour
{
    // =========================================================================
    // Inspector-serializable settings classes
    // =========================================================================

    [System.Serializable]
    public class OutlineSettings
    {
        [Tooltip("Renderers that display the holographic outline effect.\n" +
                 "These should be separate outline meshes or objects with outline shaders.\n" +
                 "Start them DISABLED — this controller manages their enabled state.")]
        public Renderer[] outlineRenderers;

        [Tooltip("Shader property name for the outline's master intensity (0 = invisible, 1 = full).")]
        public string intensityProperty = "_OutlineIntensity";

        [Tooltip("Shader property name for the outline color.")]
        public string colorProperty = "_OutlineColor";

        [Space(4)]
        [Tooltip("Holographic teal — shown on initial detection.")]
        public Color detectedColor = new Color(0.0f, 0.9f, 1.0f, 0.55f);

        [Tooltip("Bright warning orange — shown when fully locked.")]
        public Color lockedColor = new Color(1.0f, 0.35f, 0.0f, 1.0f);

        [Tooltip("How quickly the outline color transitions between states.")]
        [Range(1f, 30f)]
        public float colorLerpSpeed = 10f;

        [Tooltip("How quickly the outline fades IN when first detected (intensity 0→1).")]
        [Range(1f, 30f)]
        public float fadeInSpeed = 12f;
    }

    [System.Serializable]
    public class ProgressRingSettings
    {
        [Tooltip("Renderers whose materials have a scan-progress shader property.\n" +
                 "These can be ring meshes, quads, or the same object as the outline.")]
        public Renderer[] progressRenderers;

        [Tooltip("Shader property name for the fill amount (0..1).")]
        public string progressProperty = "_ScanProgress";

        [Tooltip("Shader property name for the ring color.")]
        public string progressColorProperty = "_ProgressColor";

        [Space(4)]
        [Tooltip("Ring color while scanning (filling up).")]
        public Color scanningColor = new Color(0.0f, 1.0f, 0.45f, 1.0f);

        [Tooltip("Ring color when lock is fully achieved.")]
        public Color lockedColor = new Color(1.0f, 0.15f, 0.0f, 1.0f);
    }

    [System.Serializable]
    public class LockFeedbackSettings
    {
        [Tooltip("GameObjects to SetActive(true) on lock (VFX prefabs, particle holders, etc.).\n" +
                 "Should start DISABLED in the scene.")]
        public GameObject[] lockVFXObjects;

        [Tooltip("ParticleSystems to Play() on lock (in addition to or instead of lockVFXObjects).")]
        public ParticleSystem[] lockParticles;

        [Space(4)]
        [Tooltip("How many outline pulses fire when lock is achieved.")]
        [Range(1, 8)]
        public int pulseCount = 3;

        [Tooltip("Speed of each pulse cycle (higher = faster flicker).")]
        [Range(1f, 20f)]
        public float pulseSpeed = 7f;

        [Tooltip("How much each pulse amplifies the outline intensity above 1.")]
        [Range(0f, 1f)]
        public float pulseAmplitude = 0.4f;
    }

    [System.Serializable]
    public class FadeOutSettings
    {
        [Tooltip("Total duration of the fade-out animation.")]
        [Range(0.05f, 3f)]
        public float duration = 0.5f;

        [Tooltip("Controls the fade shape over time:\n" +
                 "  X axis = normalized time (0..1)\n" +
                 "  Y axis = intensity (1 = full, 0 = gone)\n" +
                 "Default curve eases out quickly at the start.")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    // =========================================================================
    // Inspector fields
    // =========================================================================

    [Header("Outline")]
    public OutlineSettings outline = new();

    [Header("Progress Ring")]
    public ProgressRingSettings progressRing = new();

    [Header("Lock Feedback")]
    public LockFeedbackSettings lockFeedback = new();

    [Header("Fade Out")]
    public FadeOutSettings fadeOut = new();

    // =========================================================================
    // Private state
    // =========================================================================

    private enum VisualState { Idle, Detected, Scanning, Locked, FadingOut }
    private VisualState _state = VisualState.Idle;

    // Cached instanced materials (one per renderer — avoids modifying shared assets)
    private Material[] _outlineMats;
    private Material[] _progressMats;

    // Lerp targets for smooth color/intensity transitions
    private Color   _currentOutlineColor;
    private Color   _targetOutlineColor;
    private float   _currentOutlineIntensity;   // actual value applied to shader
    private float   _targetOutlineIntensity;    // what we're lerping toward

    // Remembered for fade-out interpolation
    private float   _currentProgress;
    private Color   _currentProgressColor;

    // Active coroutine handles
    private Coroutine _fadeRoutine;
    private Coroutine _pulseRoutine;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        _outlineMats  = InstantiateMaterials(outline.outlineRenderers);
        _progressMats = InstantiateMaterials(progressRing.progressRenderers);

        // Ensure all outline renderers start disabled
        SetOutlineRenderersEnabled(false);

        // Initialize shader values
        ApplyOutline(Color.clear, 0f);
        ApplyProgress(0f, Color.clear);

        _currentOutlineColor     = Color.clear;
        _targetOutlineColor      = Color.clear;
        _currentOutlineIntensity = 0f;
        _targetOutlineIntensity  = 0f;
    }

    /// <summary>
    /// Handles smooth color/intensity lerping every frame.
    /// Only active during Detected, Scanning, and Locked states.
    /// </summary>
    private void Update()
    {
        if (_state == VisualState.Idle || _state == VisualState.FadingOut)
            return;

        float dt = Time.deltaTime;

        // Lerp outline color
        _currentOutlineColor = Color.Lerp(
            _currentOutlineColor,
            _targetOutlineColor,
            dt * outline.colorLerpSpeed
        );

        // Lerp outline intensity (fade-in on detection, jump on state changes)
        _currentOutlineIntensity = Mathf.Lerp(
            _currentOutlineIntensity,
            _targetOutlineIntensity,
            dt * outline.fadeInSpeed
        );

        ApplyOutline(_currentOutlineColor, _currentOutlineIntensity);
    }

    private void OnDestroy()
    {
        // Clean up instanced materials to prevent leaks
        DestroyMaterials(_outlineMats);
        DestroyMaterials(_progressMats);
    }

    // =========================================================================
    // Public API — called by GazeTargetBehaviour
    // =========================================================================

    /// <summary>
    /// Gaze first acquired this target.
    /// Shows a thin holographic outline at low intensity (fades in smoothly).
    /// </summary>
    public void ShowDetected()
    {
        CancelAllCoroutines();
        _state = VisualState.Detected;

        _currentProgress      = 0f;
        _currentProgressColor = progressRing.scanningColor;

        // Enable renderers — intensity starts at 0 and lerps to 1 in Update
        SetOutlineRenderersEnabled(true);
        _targetOutlineColor     = outline.detectedColor;
        _targetOutlineIntensity = 1f;
        // Don't snap _currentOutlineIntensity — let it fade in from whatever it is

        ApplyProgress(0f, progressRing.scanningColor);
    }

    /// <summary>
    /// Updates the progress ring fill (0..1).
    /// Called every frame by DetectionVisualActor while this is the active target.
    /// </summary>
    public void UpdateProgress(float progress)
    {
        // Don't override locked visuals mid-lock
        if (_state == VisualState.Locked || _state == VisualState.FadingOut)
            return;

        _currentProgress      = progress;
        _currentProgressColor = progressRing.scanningColor;
        _state = progress > 0.001f ? VisualState.Scanning : VisualState.Detected;

        ApplyProgress(progress, progressRing.scanningColor);
    }

    /// <summary>
    /// Full lock achieved.
    /// Transitions outline to locked color, fills ring, activates VFX, fires pulse.
    /// </summary>
    public void ShowLocked()
    {
        CancelAllCoroutines();
        _state = VisualState.Locked;

        _currentProgress      = 1f;
        _currentProgressColor = progressRing.lockedColor;

        // Transition outline to locked color
        _targetOutlineColor     = outline.lockedColor;
        _targetOutlineIntensity = 1f;

        ApplyProgress(1f, progressRing.lockedColor);
        ActivateLockVFX();

        _pulseRoutine = StartCoroutine(PulseRoutine());
    }

    /// <summary>
    /// Triggers the visual fade-out coroutine.
    /// Called by GazeTargetBehaviour after the DetectionVisualActor's fade delay.
    /// </summary>
    public void StartFadeOut()
    {
        if (_state == VisualState.Idle) return; // already hidden

        CancelAllCoroutines();
        _state = VisualState.FadingOut;
        _fadeRoutine = StartCoroutine(FadeOutRoutine());
    }

    // =========================================================================
    // Coroutines
    // =========================================================================

    private IEnumerator FadeOutRoutine()
    {
        float startIntensity     = _currentOutlineIntensity;
        float startProgressValue = _currentProgress;
        Color startOutlineColor  = _currentOutlineColor;
        Color startProgressColor = _currentProgressColor;
        float elapsed = 0f;

        while (elapsed < fadeOut.duration)
        {
            elapsed += Time.deltaTime;
            float t     = Mathf.Clamp01(elapsed / fadeOut.duration);
            float eased = fadeOut.curve.Evaluate(t);

            float intensity = Mathf.Lerp(startIntensity,     0f,          eased);
            float prog      = Mathf.Lerp(startProgressValue, 0f,          eased);
            Color outCol    = Color.Lerp(startOutlineColor,  Color.clear, eased);
            Color ringCol   = Color.Lerp(startProgressColor, Color.clear, eased);

            // Write directly — bypass the Update lerp while fading
            _currentOutlineColor     = outCol;
            _currentOutlineIntensity = intensity;

            ApplyOutline(outCol, intensity);
            ApplyProgress(prog,  ringCol);

            yield return null;
        }

        // Fully hidden — reset state
        ApplyOutline(Color.clear, 0f);
        ApplyProgress(0f, Color.clear);
        SetOutlineRenderersEnabled(false);
        DeactivateLockVFX();

        _currentOutlineIntensity = 0f;
        _currentOutlineColor     = Color.clear;
        _state = VisualState.Idle;
        _fadeRoutine = null;
    }

    private IEnumerator PulseRoutine()
    {
        for (int i = 0; i < lockFeedback.pulseCount; i++)
        {
            // Rise
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * lockFeedback.pulseSpeed;
                t  = Mathf.Clamp01(t);
                float pulse = Mathf.Sin(t * Mathf.PI) * lockFeedback.pulseAmplitude;
                ApplyOutline(_currentOutlineColor, 1f + pulse);
                yield return null;
            }
        }

        // Restore clean locked intensity
        ApplyOutline(_currentOutlineColor, 1f);
        _currentOutlineIntensity = 1f;
        _pulseRoutine = null;
    }

    // =========================================================================
    // Private helpers — material application
    // =========================================================================

    private void ApplyOutline(Color color, float intensity)
    {
        foreach (var mat in _outlineMats)
        {
            if (mat == null) continue;
            if (mat.HasProperty(outline.colorProperty))
                mat.SetColor(outline.colorProperty, color);
            if (mat.HasProperty(outline.intensityProperty))
                mat.SetFloat(outline.intensityProperty, intensity);
        }
    }

    private void ApplyProgress(float progress, Color color)
    {
        foreach (var mat in _progressMats)
        {
            if (mat == null) continue;
            if (mat.HasProperty(progressRing.progressProperty))
                mat.SetFloat(progressRing.progressProperty, progress);
            if (mat.HasProperty(progressRing.progressColorProperty))
                mat.SetColor(progressRing.progressColorProperty, color);
        }
    }

    private void SetOutlineRenderersEnabled(bool enabled)
    {
        if (outline.outlineRenderers == null) return;
        foreach (var r in outline.outlineRenderers)
            if (r != null) r.enabled = enabled;
    }

    // =========================================================================
    // Private helpers — VFX
    // =========================================================================

    private void ActivateLockVFX()
    {
        if (lockFeedback.lockVFXObjects != null)
            foreach (var go in lockFeedback.lockVFXObjects)
                if (go != null) go.SetActive(true);

        if (lockFeedback.lockParticles != null)
            foreach (var ps in lockFeedback.lockParticles)
                if (ps != null) ps.Play();
    }

    private void DeactivateLockVFX()
    {
        if (lockFeedback.lockVFXObjects != null)
            foreach (var go in lockFeedback.lockVFXObjects)
                if (go != null) go.SetActive(false);

        if (lockFeedback.lockParticles != null)
            foreach (var ps in lockFeedback.lockParticles)
                if (ps != null) ps.Stop();
    }

    // =========================================================================
    // Private helpers — material management
    // =========================================================================

    /// <summary>
    /// Creates a per-instance Material copy for each renderer.
    /// This prevents modifying shared assets when we write to mat properties.
    /// </summary>
    private static Material[] InstantiateMaterials(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
            return System.Array.Empty<Material>();

        var mats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                mats[i] = renderers[i].material; // .material already returns a new instance
        }
        return mats;
    }

    private static void DestroyMaterials(Material[] mats)
    {
        if (mats == null) return;
        foreach (var mat in mats)
            if (mat != null) Destroy(mat);
    }

    private void CancelAllCoroutines()
    {
        if (_fadeRoutine  != null) { StopCoroutine(_fadeRoutine);  _fadeRoutine  = null; }
        if (_pulseRoutine != null) { StopCoroutine(_pulseRoutine); _pulseRoutine = null; }
    }
}
