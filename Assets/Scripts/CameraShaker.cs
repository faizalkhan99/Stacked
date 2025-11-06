using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    [Tooltip("How long the shake effect lasts.")]
    [SerializeField] private float _shakeDuration;
    [Tooltip("How intense the shake is.")]
    [SerializeField] private float _shakeMagnitude;

    [Header("Shake Range Settings")]
    [SerializeField] private float _shakeXMax;
    [SerializeField] private float _shakeXMin;
    [SerializeField] private float _shakeYMin;
    [SerializeField] private float _shakeYMax;

    private Vector3 _originalPosition;
    private Coroutine _shakeCoroutine;

    void Start()
    {
        _originalPosition = transform.localPosition;
    }

    // Public method to be called by GameManager
    public void Shake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            transform.localPosition = _originalPosition;
        }

        _originalPosition = transform.localPosition;   // ← capture live position now
        _shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }


    private IEnumerator ShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _shakeDuration)
        {
            // Create a random offset
            float xOffset = Random.Range(_shakeXMin, _shakeXMax) * _shakeMagnitude;
            float yOffset = Random.Range(_shakeYMin, _shakeYMax) * _shakeMagnitude;
            // Apply the offset from the original position
            transform.localPosition = _originalPosition + new Vector3(xOffset, yOffset, 0);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // When done, snap back to the original position
        transform.localPosition = _originalPosition;
        _shakeCoroutine = null;
    }
}