using UnityEngine;
using System.Collections;

/// <summary>
/// Perfect drop ring FX:
/// 1. Spawns at (blockScale * spawnMultiplier)
/// 2. Holds for holdDuration
/// 3. Scales up to (blockScale * scaleMultiplier) while fading out
/// Fully pool-safe and shader-agnostic.
/// </summary>
public class PerfectImpactFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform ringPlane;
    [SerializeField] private ParticleSystem sparkBurst;

    [Header("Timing & Scale Settings")]
    [Tooltip("Initial size relative to block scale when spawned.")]
    [SerializeField] private float spawnMultiplier = 1.0f;

    [Tooltip("Final expanded size relative to block scale.")]
    [SerializeField] private float scaleMultiplier = 1.4f;

    [Tooltip("How long the ring stays before starting to scale up.")]
    [SerializeField] private float holdDuration = 0.15f;

    [Tooltip("How long it takes to scale and fade out.")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Tooltip("Total lifetime of this FX instance (for pooling).")]
    [SerializeField] private float lifeTime = 0.8f;

    private Material _ringMat;
    private Coroutine _fxRoutine;
    private Color _initialColor;

    public float Lifetime => lifeTime;

    private void Awake()
    {
        if (ringPlane != null)
        {
            var renderer = ringPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Clone material to avoid shared instance side effects in pooled FX
                _ringMat = new Material(renderer.sharedMaterial);
                renderer.material = _ringMat;
                _initialColor = GetMaterialColor(_ringMat);
            }
        }
    }

    public void Initialize(Vector3 blockScale)
    {
        if (_fxRoutine != null)
            StopCoroutine(_fxRoutine);

        // Reset material color and scale each reuse
        SetMaterialColor(_ringMat, _initialColor);
        transform.rotation = Quaternion.identity;

        _fxRoutine = StartCoroutine(PlayRingFX(blockScale));

        if (sparkBurst != null)
        {
            sparkBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            sparkBurst.Play();
        }
    }

    private IEnumerator PlayRingFX(Vector3 blockScale)
    {
        if (ringPlane == null || _ringMat == null)
            yield break;

        // Scales
        Vector3 startScale = blockScale * spawnMultiplier;
        Vector3 endScale = blockScale * scaleMultiplier;

        ringPlane.localScale = new Vector3(startScale.x, startScale.z, 1f);

        // Colors
        Color startColor = GetMaterialColor(_ringMat);
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        // --- Hold phase ---
        float elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- Expand + Fade ---
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;

            Vector3 currentScale = Vector3.Lerp(startScale, endScale, t);
            ringPlane.localScale = new Vector3(currentScale.x, currentScale.z, 1f);

            SetMaterialColor(_ringMat, Color.Lerp(startColor, endColor, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure end state
        ringPlane.localScale = new Vector3(endScale.x, 1f, endScale.z);
        SetMaterialColor(_ringMat, endColor);

        _fxRoutine = null;
    }

    // --- shader-safe color access ---
    private static Color GetMaterialColor(Material mat)
    {
        if (mat == null) return Color.white;
        if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_TintColor")) return mat.GetColor("_TintColor");
        return Color.white;
    }

    private static void SetMaterialColor(Material mat, Color color)
    {
        if (mat == null) return;
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", color);
    }
}
