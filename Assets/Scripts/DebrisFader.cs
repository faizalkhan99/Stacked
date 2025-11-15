using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class DebrisFader : MonoBehaviour
{
    private Coroutine _fadeCoroutine;
    private Rigidbody _rb; // Cache the Rigidbody

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Call this to start the fade and return-to-pool process
    public void StartFade(float duration)
    {
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
            float t = Mathf.Clamp01(timer / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // Ensure final scale is zero
        transform.localScale = endScale;

        // Reset the Rigidbody state
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // Return to the pool
        DebrisPooler.Instance.ReturnDebris(gameObject);
    }
}
