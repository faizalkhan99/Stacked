using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [Header("Build Settings")]
    [Tooltip("If OFF, this FPS display will not appear in builds.")]
    public bool includeInBuild = true;

    [Header("UI Settings")]
    [Tooltip("Assign a Text or TMP_Text component from your Canvas.")]
    public TMP_Text fpsText;

    [Header("Settings")]
    [Tooltip("How often to update FPS display (in seconds). Lower = more responsive, higher = more stable.")]
    public float refreshRate = 0.5f;

    [Tooltip("Enable color coding based on FPS value.")]
    public bool useColorCoding = true;

    private int frameCount = 0;
    private float timer = 0f;
    private float fps = 0f;

    void Awake()
    {
        // 🧩 Hide or destroy in builds if excluded
        #if !UNITY_EDITOR
        if (!includeInBuild)
        {
            if (fpsText != null)
                Destroy(fpsText.gameObject); // completely remove from scene hierarchy
            Destroy(gameObject); // remove the FPSDisplay object itself
            return;
        }
        #endif

        // ✅ Always visible in editor for debugging
        if (fpsText != null)
            fpsText.gameObject.SetActive(true);
    }

    void Update()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;

        if (timer >= refreshRate)
        {
            fps = frameCount / timer;
            frameCount = 0;
            timer = 0f;

            if (fpsText != null)
            {
                fpsText.text = $"{fps:0} FPS";

                if (useColorCoding)
                    fpsText.color = GetColorForFPS(fps);
            }
        }
    }

    private Color GetColorForFPS(float fps)
    {
        // 💚 Green = Good | 💛 Yellow = Okay | ❤️ Red = Poor
        if (fps >= 50f)
            return new Color(0.2f, 1f, 0.2f); // Green
        else if (fps >= 30f)
            return new Color(1f, 0.85f, 0.2f); // Yellow
        else
            return new Color(1f, 0.3f, 0.3f); // Red
    }
}
