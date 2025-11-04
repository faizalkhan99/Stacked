using UnityEngine;
using System.Collections;

public class DebrisFader : MonoBehaviour
{
    private Coroutine _fadeCoroutine;

    // Call this to start the fade and destroy process
    public void StartFade(float duration)
    {
        // Stop any existing fade just in case
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeOut(duration));
    }

    private IEnumerator FadeOut(float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Use a simple Lerp for a linear scale down
            float t = Mathf.Clamp01(timer / duration);

            // Apply the new scale
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null; // Wait for the next frame
        }

        // Ensure final scale is zero and destroy the object
        transform.localScale = endScale;
        Destroy(gameObject);
    }
}
