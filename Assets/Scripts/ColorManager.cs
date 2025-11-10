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
    [Header("Block Color Settings")]
    public bool enableBlockGradient = true;
    public Color defaultBlockColor = Color.white;
    public int minGradientLength = 10;
    public int maxGradientLength = 25;

    private System.Random random = new System.Random();
    private Color[] platformColors = new Color[3];
    private int lastPlatformIndexForBlockColor = 0;
    private int blockColorsAmount = 0;
    private int blockColorBlocksLength = 0;

    [Header("Skybox Settings")]
    public bool enableBackgroundTransition = true;
    public List<SkyboxGradientPreset> skyboxPresets = new List<SkyboxGradientPreset>();
    public float crossFadeDuration = 5.0f;
    public float backgroundColorDelay = 10.0f;

    [Header("Gradient Controls")]
    [Tooltip("Adjusts where the gradient midpoint lies. 0 = TopColor only, 100 = BottomColor only.")]
    [Range(0f, 100f)] public float gradientHeight = 50f;

    [Tooltip("Controls how strong the blend between top and bottom is.")]
    [Range(0.01f, 5f)] public float gradientBlendStrength = 1f;

    private readonly int _topColorID = Shader.PropertyToID("_TopColor");
    private readonly int _bottomColorID = Shader.PropertyToID("_BottomColor");
    private readonly int _gradientHeightID = Shader.PropertyToID("_GradientHeight");
    private readonly int _blendStrengthID = Shader.PropertyToID("_BlendStrength");

    private Material _skyboxMaterialInstance;
    private Coroutine _backgroundCoroutine;
    private int _currentSkyboxPresetIndex = 0;

    void Awake()
    {
        if (RenderSettings.skybox != null)
        {
            _skyboxMaterialInstance = new Material(RenderSettings.skybox);
            RenderSettings.skybox = _skyboxMaterialInstance;
            Debug.Log("Skybox Material Instance created and assigned.");
        }
        else
        {
            Debug.LogError("ColorManager: No Skybox material assigned in Render Settings (Lighting > Environment)!");
        }
    }

    void Start() => InitializeColors();

    public void InitializeColors()
    {
        SetPlatformBlockColors(0);
        if (!_skyboxMaterialInstance) return;

        if (skyboxPresets == null || skyboxPresets.Count == 0)
        {
            Debug.LogWarning("ColorManager: No Skybox Presets defined.");
            enableBackgroundTransition = false;
        }

        if (enableBackgroundTransition && skyboxPresets.Count > 0)
        {
            _currentSkyboxPresetIndex = 0;
            ApplyGradientToSkybox(skyboxPresets[0].topColor, skyboxPresets[0].bottomColor);

            if (_backgroundCoroutine != null) StopCoroutine(_backgroundCoroutine);
            _backgroundCoroutine = StartCoroutine(ChangeBackgroundColor());
        }
        else if (skyboxPresets.Count > 0)
        {
            ApplyGradientToSkybox(skyboxPresets[0].topColor, skyboxPresets[0].bottomColor);
        }
        else
        {
            ApplyGradientToSkybox(Color.cyan, Color.blue);
        }
    }

    private IEnumerator ChangeBackgroundColor()
    {
        if (!_skyboxMaterialInstance || skyboxPresets.Count < 2)
        {
            yield break;
        }

        while (enableBackgroundTransition)
        {
            yield return new WaitForSeconds(backgroundColorDelay);
            if (!_skyboxMaterialInstance) yield break;

            int startIndex = _currentSkyboxPresetIndex;
            int nextIndex = (_currentSkyboxPresetIndex + 1) % skyboxPresets.Count;

            Color startTop = skyboxPresets[startIndex].topColor;
            Color startBottom = skyboxPresets[startIndex].bottomColor;
            Color endTop = skyboxPresets[nextIndex].topColor;
            Color endBottom = skyboxPresets[nextIndex].bottomColor;

            float timer = 0f;

            while (timer < crossFadeDuration)
            {
                if (!_skyboxMaterialInstance) yield break;
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / crossFadeDuration);

                Color currentTop = Color.Lerp(startTop, endTop, t);
                Color currentBottom = Color.Lerp(startBottom, endBottom, t);
                ApplyGradientToSkybox(currentTop, currentBottom);

                yield return null;
            }

            ApplyGradientToSkybox(endTop, endBottom);
            _currentSkyboxPresetIndex = nextIndex;
        }
    }

    private void ApplyGradientToSkybox(Color top, Color bottom)
    {
        if (!_skyboxMaterialInstance) return;

        _skyboxMaterialInstance.SetColor(_topColorID, top);
        _skyboxMaterialInstance.SetColor(_bottomColorID, bottom);
        _skyboxMaterialInstance.SetFloat(_gradientHeightID, gradientHeight / 100f);
        _skyboxMaterialInstance.SetFloat(_blendStrengthID, gradientBlendStrength);
    }

    public Color GetCurrentBlockColor(int score)
    {
        if (enableBlockGradient)
        {
            if (score >= blockColorsAmount) SetPlatformBlockColors(score);
            float t = (blockColorBlocksLength > 1)
                ? (float)(score - lastPlatformIndexForBlockColor) / (blockColorBlocksLength - 1)
                : 0f;

            return Color.Lerp(platformColors[0], platformColors[1], Mathf.Clamp01(t));
        }
        return defaultBlockColor;
    }

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
        Random.ColorHSV(0.0f, 1.0f, 0.7f, 1.0f, 0.8f, 1.0f);

    void OnDestroy()
    {
        if (_skyboxMaterialInstance != null)
        {
            Destroy(_skyboxMaterialInstance);
            Debug.Log("Destroyed Skybox Material Instance.");
        }
    }
}
