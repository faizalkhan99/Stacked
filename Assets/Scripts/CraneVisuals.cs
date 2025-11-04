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

    [Header("Movement Smoothing")] // NEW Section
    [Tooltip("How quickly the pivot moves up to its new target height.")]
    public float pivotMoveSpeed = 5f;

    // Swing parameters
    private float _speed;
    private float _swingDistance;
    private float _ropeLength;
    private Vector3 _swingAxis;
    //private Vector3 _pivotPoint; // This is a Vector3
    private float _targetPivotY; // NEW: Target height for smooth movement
    private float _startTime;
    private bool _isSwinging = false;

    private LineRenderer _lineRenderer;

    void Awake()
    {
        // Get LineRenderer here now, it's safe
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            Debug.LogError("CraneVisuals missing LineRenderer!");
            enabled = false;
            return;
        }
        _lineRenderer.positionCount = chainResolution;


        if (clawTransform == null)
        {
            Debug.LogError("CraneVisuals: 'clawTransform' is NOT ASSIGNED!");
        }
        else
        {
            clawTransform.gameObject.SetActive(false); // Hide at start
        }
        _lineRenderer.enabled = false; // Hide line at start

        // Initialize target height to current position
        _targetPivotY = transform.position.y;
    }

    public void Initialize(float speed, float swingDistance, float ropeLength, Vector3 swingAxis)
    {
        if (_lineRenderer == null) return;

        _speed = speed;
        _swingDistance = swingDistance;
        _ropeLength = ropeLength;
        _swingAxis = swingAxis.normalized;
        _startTime = Time.time;
        _isSwinging = true;

        if (clawTransform != null) clawTransform.gameObject.SetActive(true);
        _lineRenderer.enabled = true;
    }
    // Sets the target height the pivot should move towards
    public void SetTargetHeight(float newY)
    {
        _targetPivotY = newY;
    }
    void LateUpdate()
    {
        // --- 1. Smoothly Move Pivot Vertically ---
        Vector3 currentPosition = transform.position;
        float newY = Mathf.Lerp(currentPosition.y, _targetPivotY, Time.deltaTime * pivotMoveSpeed);
        // Only update Y position, keep X/Z from editor/start
        transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        // ---

        if (!_isSwinging) return; // Don't swing/draw chain if hidden/stopped

        // --- 2. Swing Claw (using CURRENT pivot position) ---
        Vector3 currentPivotPos = transform.position; // Read the updated position
        float timeElapsed = (Time.time - _startTime) * _speed;
        float pathPosition = Mathf.Sin(timeElapsed) * _swingDistance;

        // Ensure yPos calculation doesn't go invalid if ropeLength is very small
        float yOffsetCalc = Mathf.Pow(_ropeLength, 2) - Mathf.Pow(pathPosition, 2);
        float yPos = currentPivotPos.y - Mathf.Sqrt(Mathf.Max(0f, yOffsetCalc)); // Use Max to prevent sqrt of negative

        Vector3 clawPosition = currentPivotPos + (_swingAxis * pathPosition);
        clawPosition.y = yPos;

        if (clawTransform != null) clawTransform.position = clawPosition;
        else return; // Cant draw chain without claw

        // --- 3. Draw Chain ---
        Vector3 pivotForChain = currentPivotPos; // Use updated pivot pos
        Vector3 midPoint = (pivotForChain + clawPosition) / 2f;
        midPoint += Vector3.down * chainSag;

        if (_lineRenderer) // Check if line renderer exists
        {
            for (int i = 0; i < chainResolution; i++)
            {
                float t = (i == chainResolution - 1) ? 1f : (float)i / (chainResolution - 1); // Ensure t reaches exactly 1
                Vector3 m1 = Vector3.Lerp(pivotForChain, midPoint, t);
                Vector3 m2 = Vector3.Lerp(midPoint, clawPosition, t);
                Vector3 curvePos = Vector3.Lerp(m1, m2, t);
                if (i < _lineRenderer.positionCount) // Safety check
                   _lineRenderer.SetPosition(i, curvePos);
            }
        }
    }

    public void Hide()
    {
        _isSwinging = false;
        //if (clawTransform != null) clawTransform.gameObject.SetActive(false);
        //if (_lineRenderer != null) _lineRenderer.enabled = false;
    }
}