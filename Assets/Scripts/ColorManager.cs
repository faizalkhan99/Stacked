// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// [System.Serializable]
// public struct SkyboxGradientPreset
// {
//     [ColorUsage(true, true)] public Color topColor;
//     [ColorUsage(true, true)] public Color bottomColor;
// }

// public class ColorManager : MonoBehaviour
// {
//     [Header("Block Color Settings")]
//     public bool enableBlockGradient = true;
//     public Color defaultBlockColor = Color.white;
//     public int minGradientLength = 10;
//     public int maxGradientLength = 25;

//     private System.Random random = new System.Random();
//     private Color[] platformColors = new Color[3];
//     private int lastPlatformIndexForBlockColor = 0;
//     private int blockColorsAmount = 0;
//     private int blockColorBlocksLength = 0;

//     [Header("Skybox Settings")]
//     public bool enableBackgroundTransition = true;
//     public List<SkyboxGradientPreset> skyboxPresets = new List<SkyboxGradientPreset>();
//     public float crossFadeDuration = 5.0f;
//     public float backgroundColorDelay = 10.0f;

//     [Header("Gradient Controls")]
//     [Tooltip("Adjusts where the gradient midpoint lies. 0 = TopColor only, 100 = BottomColor only.")]
//     [Range(0f, 100f)] public float gradientHeight = 50f;

//     [Tooltip("Controls how strong the blend between top and bottom is.")]
//     [Range(0.01f, 5f)] public float gradientBlendStrength = 1f;

//     private readonly int _topColorID = Shader.PropertyToID("_TopColor");
//     private readonly int _bottomColorID = Shader.PropertyToID("_BottomColor");
//     private readonly int _gradientHeightID = Shader.PropertyToID("_GradientHeight");
//     private readonly int _blendStrengthID = Shader.PropertyToID("_BlendStrength");

//     private Material _skyboxMaterialInstance;
//     private Coroutine _backgroundCoroutine;
//     private int _currentSkyboxPresetIndex = 0;

//     void Awake()
//     {
//         if (RenderSettings.skybox != null)
//         {
//             _skyboxMaterialInstance = new Material(RenderSettings.skybox);
//             RenderSettings.skybox = _skyboxMaterialInstance;
//             Debug.Log("Skybox Material Instance created and assigned.");
//         }
//         else
//         {
//             Debug.LogError("ColorManager: No Skybox material assigned in Render Settings (Lighting > Environment)!");
//         }
//     }

//     void Start() => InitializeColors();

//     public void InitializeColors()
//     {
//         SetPlatformBlockColors(0);
//         if (!_skyboxMaterialInstance) return;

//         if (skyboxPresets == null || skyboxPresets.Count == 0)
//         {
//             Debug.LogWarning("ColorManager: No Skybox Presets defined.");
//             enableBackgroundTransition = false;
//         }

//         if (enableBackgroundTransition && skyboxPresets.Count > 0)
//         {
//             _currentSkyboxPresetIndex = 0;
//             ApplyGradientToSkybox(skyboxPresets[0].topColor, skyboxPresets[0].bottomColor);

//             if (_backgroundCoroutine != null) StopCoroutine(_backgroundCoroutine);
//             _backgroundCoroutine = StartCoroutine(ChangeBackgroundColor());
//         }
//         else if (skyboxPresets.Count > 0)
//         {
//             ApplyGradientToSkybox(skyboxPresets[0].topColor, skyboxPresets[0].bottomColor);
//         }
//         else
//         {
//             ApplyGradientToSkybox(Color.cyan, Color.blue);
//         }
//     }

//     private IEnumerator ChangeBackgroundColor()
//     {
//         if (!_skyboxMaterialInstance || skyboxPresets.Count < 2)
//         {
//             yield break;
//         }

//         while (enableBackgroundTransition)
//         {
//             yield return new WaitForSeconds(backgroundColorDelay);
//             if (!_skyboxMaterialInstance) yield break;

//             int startIndex = _currentSkyboxPresetIndex;
//             int nextIndex = (_currentSkyboxPresetIndex + 1) % skyboxPresets.Count;

//             Color startTop = skyboxPresets[startIndex].topColor;
//             Color startBottom = skyboxPresets[startIndex].bottomColor;
//             Color endTop = skyboxPresets[nextIndex].topColor;
//             Color endBottom = skyboxPresets[nextIndex].bottomColor;

//             float timer = 0f;

//             while (timer < crossFadeDuration)
//             {
//                 if (!_skyboxMaterialInstance) yield break;
//                 timer += Time.deltaTime;
//                 float t = Mathf.Clamp01(timer / crossFadeDuration);

//                 Color currentTop = Color.Lerp(startTop, endTop, t);
//                 Color currentBottom = Color.Lerp(startBottom, endBottom, t);
//                 ApplyGradientToSkybox(currentTop, currentBottom);

//                 yield return null;
//             }

//             ApplyGradientToSkybox(endTop, endBottom);
//             _currentSkyboxPresetIndex = nextIndex;
//         }
//     }

//     private void ApplyGradientToSkybox(Color top, Color bottom)
//     {
//         if (!_skyboxMaterialInstance) return;

//         _skyboxMaterialInstance.SetColor(_topColorID, top);
//         _skyboxMaterialInstance.SetColor(_bottomColorID, bottom);
//         _skyboxMaterialInstance.SetFloat(_gradientHeightID, gradientHeight / 100f);
//         _skyboxMaterialInstance.SetFloat(_blendStrengthID, gradientBlendStrength);
//     }

//     public Color GetCurrentBlockColor(int score)
//     {
//         if (enableBlockGradient)
//         {
//             if (score >= blockColorsAmount) SetPlatformBlockColors(score);
//             float t = (blockColorBlocksLength > 1)
//                 ? (float)(score - lastPlatformIndexForBlockColor) / (blockColorBlocksLength - 1)
//                 : 0f;

//             return Color.Lerp(platformColors[0], platformColors[1], Mathf.Clamp01(t));
//         }
//         return defaultBlockColor;
//     }

//     private void SetPlatformBlockColors(int currentIndex)
//     {
//         blockColorBlocksLength = random.Next(minGradientLength, maxGradientLength + 1);
//         blockColorsAmount += blockColorBlocksLength;
//         lastPlatformIndexForBlockColor = currentIndex;
//         platformColors[0] = (currentIndex == 0) ? GetRandomColor() : platformColors[1];
//         platformColors[1] = (currentIndex == 0) ? GetRandomColor() : platformColors[2];
//         platformColors[2] = GetRandomColor();
//     }

//     private Color GetRandomColor() =>
//         Random.ColorHSV(0.0f, 1.0f, 0.7f, 1.0f, 0.8f, 1.0f);

//     void OnDestroy()
//     {
//         if (_skyboxMaterialInstance != null)
//         {
//             Destroy(_skyboxMaterialInstance);
//             Debug.Log("Destroyed Skybox Material Instance.");
//         }
//     }
// }

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SkyboxGradientPreset
{
    [ColorUsage(true, true)] public Color topColor;
    [ColorUsage(true, true)] public Color bottomColor;
}

public class ColorManager : MonoBehaviour
{
    // ==========================
    // BLOCK COLOR SETTINGS
    // ==========================

    [Header("Block Color Settings")]
    public bool enableBlockGradient = true;
    public Color defaultBlockColor = Color.white;

    [Tooltip("Minimum number of blocks before gradient shifts.")]
    public int minGradientLength = 10;

    [Tooltip("Maximum number of blocks before gradient shifts.")]
    public int maxGradientLength = 25;

    // Procedural gradient
    private System.Random random = new System.Random();
    private Color[] platformColors = new Color[3];
    private int lastPlatformIndexForBlockColor = 0;
    private int blockColorsAmount = 0;
    private int blockColorBlocksLength = 0;


    // ==========================
    // MANUAL PALETTE
    // ==========================

    [Header("Manual Palette Mode")]
    [Tooltip("Use manual color palette instead of procedural gradient.")]
    public bool useManualPalette = false;

    [Tooltip("Add any number of block colors manually.")]
    public List<Color> manualColorPalette = new List<Color>();


    // ==========================
    // SKYBOX SETTINGS
    // ==========================

    [Header("Skybox Settings")]
    public bool enableBackgroundTransition = true;
    public List<SkyboxGradientPreset> skyboxPresets = new List<SkyboxGradientPreset>();

    [Tooltip("Time to fully crossfade between skybox presets.")]
    public float crossFadeDuration = 5.0f;

    [Tooltip("Delay between full skybox preset transitions.")]
    public float backgroundColorDelay = 10.0f;


    [Header("Gradient Controls")]
    [Tooltip("Height of the skybox gradient (0 = top only, 100 = bottom only).")]
    [Range(0f, 100f)] public float gradientHeight = 50f;

    [Tooltip("Strength of blending between top and bottom.")]
    [Range(0.01f, 5f)] public float gradientBlendStrength = 1f;


    // ==========================
    // SKYBOX HUE FOLLOW + CINEMATIC
    // ==========================

    [Header("Dynamic Skybox Color Sync")]
    [Tooltip("How much the skybox should follow the block color hue.")]
    [Range(0f, 1f)] public float skyboxHueFollowStrength = 0.3f;

    [Tooltip("How fast the sky responds to block color changes.")]
    public float skyboxHueLerpSpeed = 1.0f;

    [Header("Cinematic Skybox Enhancement")]
    [Tooltip("Boost sky brightness slightly for premium soft look.")]
    [Range(0f, 1f)] public float skyboxBrightnessBoost = 0.2f;

    [Tooltip("Reduce saturation for cinematic look.")]
    [Range(0f, 1f)] public float skyboxDesaturate = 0.2f;


    // ==========================
    // INTERNAL SKYBOX FIELDS
    // ==========================

    private readonly int _topColorID = Shader.PropertyToID("_TopColor");
    private readonly int _bottomColorID = Shader.PropertyToID("_BottomColor");
    private readonly int _gradientHeightID = Shader.PropertyToID("_GradientHeight");
    private readonly int _blendStrengthID = Shader.PropertyToID("_BlendStrength");

    private Material _skyboxMaterialInstance;
    private Coroutine _backgroundCoroutine;
    private int _currentSkyboxPresetIndex = 0;

    private Color _currentSkyTop;
    private Color _currentSkyBottom;


    // ==========================
    // UNITY EVENTS
    // ==========================

    void Awake()
    {
        if (RenderSettings.skybox != null)
        {
            _skyboxMaterialInstance = new Material(RenderSettings.skybox);
            RenderSettings.skybox = _skyboxMaterialInstance;
        }
        else
        {
            Debug.LogError("ColorManager: No skybox material assigned!");
        }
    }

    void Start()
    {
        ShuffleSkyboxPresets();
        InitializeColors();
        
    }
private void ShuffleSkyboxPresets()
{
    if (skyboxPresets == null || skyboxPresets.Count <= 1)
        return;

    // Fisher–Yates shuffle
    for (int i = skyboxPresets.Count - 1; i > 0; i--)
    {
        int j = Random.Range(0, i + 1);
        var temp = skyboxPresets[i];
        skyboxPresets[i] = skyboxPresets[j];
        skyboxPresets[j] = temp;
    }
}
    void Update()
    {
        // Continuously apply updated skybox transition
        if (_skyboxMaterialInstance)
        {
            // Forces continuous frame-to-frame smoothness
            ApplyGradientToSkybox(
                skyboxPresets.Count > 0 ? skyboxPresets[_currentSkyboxPresetIndex].topColor : Color.cyan,
                skyboxPresets.Count > 0 ? skyboxPresets[_currentSkyboxPresetIndex].bottomColor : Color.blue
            );
        }
    }


    // ==========================
    // COLOR INITIALIZATION
    // ==========================

    public void InitializeColors()
    {
        SetPlatformBlockColors(0);

        if (!_skyboxMaterialInstance)
            return;

        if (skyboxPresets.Count == 0)
        {
            enableBackgroundTransition = false;
            ApplyGradientToSkybox(Color.cyan, Color.blue);
            return;
        }

        _currentSkyboxPresetIndex = 0;
        ApplyGradientToSkybox(skyboxPresets[0].topColor, skyboxPresets[0].bottomColor);

        if (_backgroundCoroutine != null)
            StopCoroutine(_backgroundCoroutine);

        if (enableBackgroundTransition && skyboxPresets.Count > 1)
            _backgroundCoroutine = StartCoroutine(ChangeBackgroundColor());
    }


    // ==========================
    // SKYBOX PRESET TRANSITION
    // ==========================

    private IEnumerator ChangeBackgroundColor()
    {
        while (enableBackgroundTransition)
        {
            yield return new WaitForSeconds(backgroundColorDelay);

            int nextIndex = (_currentSkyboxPresetIndex + 1) % skyboxPresets.Count;

            Color startTop = skyboxPresets[_currentSkyboxPresetIndex].topColor;
            Color startBottom = skyboxPresets[_currentSkyboxPresetIndex].bottomColor;
            Color endTop = skyboxPresets[nextIndex].topColor;
            Color endBottom = skyboxPresets[nextIndex].bottomColor;

            float t = 0f;

            while (t < crossFadeDuration)
            {
                t += Time.deltaTime;
                float lerp = Mathf.Clamp01(t / crossFadeDuration);

                Color mixedTop = Color.Lerp(startTop, endTop, lerp);
                Color mixedBottom = Color.Lerp(startBottom, endBottom, lerp);

                ApplyGradientToSkybox(mixedTop, mixedBottom);

                yield return null;
            }

            _currentSkyboxPresetIndex = nextIndex;
        }
    }


    // ==========================
    // SKYBOX APPLICATION + BLOCK HUE SYNC
    // ==========================

    private void ApplyGradientToSkybox(Color presetTop, Color presetBottom)
    {
        if (!_skyboxMaterialInstance)
            return;

        // --- STEP 1: BASE COLORS (from preset cycle)
        Color baseTop = presetTop;
        Color baseBottom = presetBottom;

        // --- STEP 2: BLOCK COLOR HUE FOLLOW
        Color blockColor = GetCurrentBlockColor(GameManager.Instance != null ? GameManager.Instance.GetHighScore() : 0);

        Color.RGBToHSV(blockColor, out float h, out _, out _);

        Color blockHueTop = Color.HSVToRGB(h, 0.25f, 1f);
        Color blockHueBottom = Color.HSVToRGB(h, 0.20f, 0.65f);

        baseTop = Color.Lerp(baseTop, blockHueTop, skyboxHueFollowStrength);
        baseBottom = Color.Lerp(baseBottom, blockHueBottom, skyboxHueFollowStrength);

        // --- STEP 3: CINEMATIC ADJUSTMENT
        baseTop = AdjustForCinematic(baseTop);
        baseBottom = AdjustForCinematic(baseBottom);

        // --- STEP 4: Smooth results
        _currentSkyTop = Color.Lerp(_currentSkyTop, baseTop, Time.deltaTime * skyboxHueLerpSpeed);
        _currentSkyBottom = Color.Lerp(_currentSkyBottom, baseBottom, Time.deltaTime * skyboxHueLerpSpeed);

        // --- STEP 5: Apply to actual skybox
        _skyboxMaterialInstance.SetColor(_topColorID, _currentSkyTop);
        _skyboxMaterialInstance.SetColor(_bottomColorID, _currentSkyBottom);
        _skyboxMaterialInstance.SetFloat(_gradientHeightID, gradientHeight / 100f);
        _skyboxMaterialInstance.SetFloat(_blendStrengthID, gradientBlendStrength);
    }

    private Color AdjustForCinematic(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);

        s = Mathf.Lerp(s, 0f, skyboxDesaturate);
        v = Mathf.Min(1f, v + skyboxBrightnessBoost);

        return Color.HSVToRGB(h, s, v);
    }


    // ==========================
    // BLOCK COLORS
    // ==========================

    public Color GetCurrentBlockColor(int score)
    {
        if (useManualPalette && manualColorPalette.Count > 0)
        {
            return manualColorPalette[score % manualColorPalette.Count];
        }

        // Stack-style procedural hue
        return GetStackHueColor(score);
    }

    private Color GetStackHueColor(int score)
    {
        float hue = (score * 0.015f) % 1f;
        float saturation = 0.35f;
        float value = 0.85f;

        return Color.HSVToRGB(hue, saturation, value);
    }


    // ==========================
    // OLD GRADIENT LOGIC (kept for compatibility)
    // ==========================

    private void SetPlatformBlockColors(int currentIndex)
    {
        blockColorBlocksLength = random.Next(minGradientLength, maxGradientLength + 1);
        blockColorsAmount += blockColorBlocksLength;
        lastPlatformIndexForBlockColor = currentIndex;

        platformColors[0] = (currentIndex == 0) ? GetRandomColor() : platformColors[1];
        platformColors[1] = (currentIndex == 0) ? GetRandomColor() : platformColors[2];
        platformColors[2] = GetRandomColor();
    }

    private Color GetRandomColor() =>
        Random.ColorHSV(0.0f, 1.0f, 0.7f, 1.0f, 0.8f, 1.0f); // kept for legacy only


    // ==========================
    // CLEANUP
    // ==========================

    void OnDestroy()
    {
        if (_skyboxMaterialInstance != null)
        {
            Destroy(_skyboxMaterialInstance);
        }
    }
}
