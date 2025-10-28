using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CraneVisuals : MonoBehaviour
{
    [Header("Visual Components")]
    [Tooltip("Drag your 'clawPrefab' object (in the hierarchy) here.")]
    public Transform clawTransform;
    [Tooltip("The vertical offset from the claw's center to where the block should attach.")]
    public float blockAttachOffset = 1.0f;

    [Header("Chain Settings")]
    [Tooltip("How many joints to use for the chain. 15-20 looks good.")]
    public int chainResolution = 20;
    [Tooltip("How much the chain should sag in the middle.")]
    public float chainSag = 1.5f;

    // Swing parameters
    private float _speed;
    private float _swingDistance;
    private float _ropeLength;
    private Vector3 _swingAxis;
    private Vector3 _pivotPoint; // This is a Vector3
    private float _startTime;
    private bool _isSwinging = false;

    private LineRenderer _lineRenderer;

    void Awake()
    {
        // We no longer get the pivotPoint here.
        // It will be updated in LateUpdate.

        if (clawTransform != null)
        {
            clawTransform.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("CraneVisuals.Awake(): 'clawTransform' is NOT ASSIGNED in the Inspector!");
        }
    }

    public void Initialize(float speed, float swingDistance, float ropeLength, Vector3 swingAxis)
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            Debug.LogError("CRITICAL ERROR: CraneVisuals is missing its LineRenderer component! Initialize() is stopping.");
            return;
        }
        _lineRenderer.positionCount = chainResolution;

        _speed = speed;
        _swingDistance = swingDistance;
        _ropeLength = ropeLength;
        _swingAxis = swingAxis.normalized;
        _startTime = Time.time;
        _isSwinging = true;

        if (clawTransform != null)
        {
            clawTransform.gameObject.SetActive(true);
        }
        _lineRenderer.enabled = true;
    }

    void LateUpdate()
    {
        if (!_isSwinging) return;

        // --- THIS IS THE FIX ---
        // Get the pivot's CURRENT position every frame.
        _pivotPoint = transform.position;
        // --- END OF FIX ---

        // 1. ALWAYS SWING THE CLAW
        float timeElapsed = (Time.time - _startTime) * _speed;
        float pathPosition = Mathf.Sin(timeElapsed) * _swingDistance;
        
        float yPos = _pivotPoint.y - Mathf.Sqrt(Mathf.Pow(_ropeLength, 2) - Mathf.Pow(pathPosition, 2));
        Vector3 clawPosition = _pivotPoint + (_swingAxis * pathPosition);
        clawPosition.y = yPos;
        clawTransform.position = clawPosition;

        // 2. ALWAYS DRAW THE FLEXIBLE CHAIN
        Vector3 pivotPosition = _pivotPoint; 
        Vector3 midPoint = (pivotPosition + clawPosition) / 2f;
        midPoint += Vector3.down * chainSag;

        for (int i = 0; i < chainResolution; i++)
        {
            float t = (float)i / (chainResolution - 1);
            Vector3 m1 = Vector3.Lerp(pivotPosition, midPoint, t);
            Vector3 m2 = Vector3.Lerp(midPoint, clawPosition, t);
            Vector3 curvePos = Vector3.Lerp(m1, m2, t);
            _lineRenderer.SetPosition(i, curvePos);
        }
    }

    public void Hide()
    {
        _isSwinging = false;
        if (clawTransform != null) clawTransform.gameObject.SetActive(false);
        if (_lineRenderer != null) _lineRenderer.enabled = false;
    }
}