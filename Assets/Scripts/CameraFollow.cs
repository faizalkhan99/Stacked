// using UnityEngine;

// public class CameraFollow : MonoBehaviour
// {
//     [Tooltip("The object the camera will follow.")]
//     public Transform target;
//     [Tooltip("How quickly the camera catches up to the target.")]
//     public float smoothSpeed = 0.125f;

//     private Vector3 _offset;
//     private Vector3 _initialPosition; // To store the starting X and Z

//     void Start()
//     {
//         _initialPosition = transform.position;
//         if (target != null)
//         {
//             _offset = transform.position - target.position;
//         }
//         Debug.Log($"CameraFollow Start: Target='{target?.name}', TargetPos={target?.position}, CameraPos={transform.position}, Calculated Offset={_offset}");
//     }

//     void LateUpdate()
//     {
//         if (target == null) return;

//         // --- THE FIX IS HERE ---
//         // 1. Calculate the desired vertical (Y) position only.
//         float desiredY = target.position.y + _offset.y;

//         // 2. Smoothly interpolate only the Y position.
//         float smoothedY = Mathf.Lerp(transform.position.y, desiredY, smoothSpeed);

//         // 3. Create the new camera position using the original X and Z, but the new Y.
//         transform.position = new Vector3(_initialPosition.x, smoothedY, _initialPosition.z);
//         Debug.Log($"CameraFollow LateUpdate: TargetPos={target.position}, Offset={_offset}, DesiredY={desiredY}, SmoothedY={smoothedY}, NewCamPos={transform.position}");
//     }

//     public void SetTarget(Transform newTarget)
//     {
//         target = newTarget;
//     }
// }




using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Following")]
    [Tooltip("The object the camera will follow during gameplay.")]
    public Transform target;
    [Tooltip("How quickly the camera catches up to the target's height.")]
    public float followSmoothSpeed = 5f; // Speed for regular following

    [Header("Zoom Out Animation")]
    [Tooltip("How long the game over zoom/move animation takes.")]
    public float zoomAnimDuration = 1.5f;
    [Tooltip("Padding added around the tower bounds when zooming out.")]
    public float zoomPadding = 2.0f;
    [Tooltip("How quickly the camera moves back during reset.")]
    public float resetSmoothSpeed = 2f; // Speed for resetting camera

    // --- Private Variables ---
    private Vector3 _initialPosition; // Cached starting position
    private Quaternion _initialRotation; // Store initial rotation too
    private float _initialFOV;      // Cached starting Field of View
    private Coroutine _cameraAnimationCoroutine; // To manage zoom/reset animations
    private Camera _camera;
    private Transform _baseBlockPlatformRef; // Store reference for Y offset

    void Start()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("CameraFollow: Could not find Main Camera!");
            enabled = false;
            return;
        }
        if (!_camera.orthographic) // Ensure it's perspective
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _initialFOV = _camera.fieldOfView; // Store initial FOV
            _baseBlockPlatformRef = GameManager.Instance?.baseBlockPlatform;
            Debug.Log($"CameraFollow Start: InitialPos={_initialPosition}, InitialFOV={_initialFOV}");
        }
        else
        {
            Debug.LogError("CameraFollow: Camera is set to Orthographic, but script expects Perspective!");
            enabled = false;
            return;
        }

        // We don't calculate offset here, just use initial position logic
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Initial Target is not assigned in Inspector. Will wait for SetTarget.");
        }
    }

    // void LateUpdate()
    // {
    //     // Only follow if we have a target AND not currently animating
    //     if (target == null || _cameraAnimationCoroutine != null || currentState != GameState.Playing) return; // Added GameState check

    //     // --- Perspective Following Logic ---
    //     // Calculate desired position: Keep initial X/Z offset, follow target Y
    //     // Calculate the current horizontal offset from the initial position
    //     Vector3 currentOffset = transform.position - target.position;
    //     // Calculate the desired position maintaining initial X/Z distance but matching target's height movement
    //     // We want the camera's Y to smoothly follow the target's Y, relative to the initial vertical offset.
    //     float initialOffsetY = _initialPosition.y - (baseBlockPlatform ? baseBlockPlatform.position.y : 0f); // Approx initial Y offset from base
    //     float desiredY = target.position.y + initialOffsetY;

    //     // Keep initial X and Z world positions, only Lerp Y
    //     Vector3 desiredPosition = new Vector3(_initialPosition.x, desiredY, _initialPosition.z);

    //     // Smoothly move towards the desired position (primarily adjusting Y)
    //     transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed * Time.deltaTime);
    //     // ---
    // }


    void LateUpdate()
    {
        // Use GameManager state access method
        // Ensure GameManager.Instance and GetCurrentState() exist
        if (target == null || _cameraAnimationCoroutine != null || GameManager.Instance.GetCurrentState() != GameState.Playing) return; // Added GameState check

        // --- Perspective Following Logic ---
        // Calculate initial offset only if we have the base platform reference
        float initialOffsetY = 0f;
        if (_baseBlockPlatformRef != null)
        {
            initialOffsetY = _initialPosition.y - _baseBlockPlatformRef.position.y;
        }
        else if (target != null)
        { // Fallback if base wasn't ready in Start
          // Try getting base platform again or estimate based on current target
            _baseBlockPlatformRef = GameManager.Instance?.baseBlockPlatform;
            if (_baseBlockPlatformRef != null)
            {
                initialOffsetY = _initialPosition.y - _baseBlockPlatformRef.position.y;
            }
            else
            {
                // Less accurate fallback: Assume current target is near base height initially
                initialOffsetY = _initialPosition.y - target.position.y;
            }
        }

        float desiredY = target.position.y + initialOffsetY;

        // Keep initial X and Z world positions, only Lerp Y
        Vector3 desiredPosition = new Vector3(_initialPosition.x, desiredY, _initialPosition.z);

        // Smoothly move towards the desired position
        // Ensure camera keeps looking at the initial angle
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed * Time.deltaTime), Quaternion.Slerp(transform.rotation, _initialRotation, followSmoothSpeed * Time.deltaTime));
        // ---
    }

    // Called by GameManager to update the target during gameplay
    public void SetTarget(Transform newTarget)
    {
        // We don't reset animation here anymore, ResetCamera handles that
        target = newTarget;
    }

    // --- NEW METHOD FOR GAME OVER ZOOM (PERSPECTIVE) ---
    public void AnimateZoomToShowTower(Bounds towerBounds)
    {
        if (_cameraAnimationCoroutine != null) StopCoroutine(_cameraAnimationCoroutine); // Stop previous anim if any
        if (!_camera) return; // Safety check

        // 1. Calculate required view dimensions
        float towerHeight = towerBounds.size.y + zoomPadding * 2f;
        float towerWidth = Mathf.Max(towerBounds.size.x, towerBounds.size.z) + zoomPadding * 2f;
        float aspectRatio = (float)Screen.width / Screen.height;

        // 2. Calculate distance needed based on FOV and larger dimension
        // FOV is vertical, so calculate distance for height first
        float distanceForHeight = (towerHeight / 2f) / Mathf.Tan(_initialFOV * 0.5f * Mathf.Deg2Rad);
        // Calculate distance needed to fit width based on horizontal FOV derived from vertical FOV and aspect ratio
        float horizontalFOV = Camera.VerticalToHorizontalFieldOfView(_initialFOV, aspectRatio);
        float distanceForWidth = (towerWidth / 2f) / Mathf.Tan(horizontalFOV * 0.5f * Mathf.Deg2Rad);
        // Use the LARGER distance to ensure everything fits
        float targetDistance = Mathf.Max(distanceForHeight, distanceForWidth);

        // 3. Calculate target camera position
        Vector3 lookAtPoint = towerBounds.center; // Point to look at
        // Calculate position by moving BACKWARDS from the look-at point along the camera's initial forward direction
        Vector3 directionVector = _initialRotation * Vector3.forward; // Get the initial forward vector
        Vector3 targetPosition = lookAtPoint - (directionVector * targetDistance);

        Debug.Log($"AnimateZoom: BoundsCenter={towerBounds.center}, BoundsSize={towerBounds.size}, TargetDist={targetDistance}, TargetPos={targetPosition}");

        // 4. Start the animation coroutine
        _cameraAnimationCoroutine = StartCoroutine(DoCameraMoveAnimation(targetPosition, _initialFOV, zoomAnimDuration, lookAtPoint)); // Keep initial FOV
    }

    // Coroutine for moving camera (position and/or FOV)
    private IEnumerator DoCameraMoveAnimation(Vector3 targetPosition, float targetFOV, float duration, Vector3? lookAtTarget = null) // Added optional lookAtTarget
    {
        target = null; // Stop following target block

        transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        float startFOV = _camera.fieldOfView;

        float timer = 0f;
        while (timer < duration)
        {
            if (!_camera) yield break;
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            _camera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            // --- Keep Looking At Target ---
            if (lookAtTarget.HasValue)
            {
                // Smoothly look towards the center of the tower
                // Ensure target isn't exactly at camera position to avoid LookRotation error
                if (Vector3.SqrMagnitude(lookAtTarget.Value - transform.position) > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget.Value - transform.position);
                    // Slerp from startRotation towards targetRotation for consistent feel
                    transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                }
            }
            else
            {
                // If no lookAt target (like during reset), smoothly return to initial rotation
                transform.rotation = Quaternion.Slerp(startRotation, _initialRotation, t);
            }
            // ---

            yield return null;
        }

        // Ensure final values are set
        transform.position = targetPosition;
        if (_camera) _camera.fieldOfView = targetFOV;
        if (lookAtTarget.HasValue) transform.LookAt(lookAtTarget.Value); // Snap final rotation if looking
        else transform.rotation = _initialRotation; // Snap to initial rotation if resetting

        _cameraAnimationCoroutine = null;
        Debug.Log("Camera Animation Complete.");
    }

    // --- NEW: Method to reset camera (animated) ---
    public void ResetCamera(Transform initialTarget)
    {
        if (_cameraAnimationCoroutine != null) StopCoroutine(_cameraAnimationCoroutine);

        // Use the initial values stored in Start()
        Vector3 targetPosition = _initialPosition;
        float targetFOV = _initialFOV;

        // Start animation back to initial state
        _cameraAnimationCoroutine = StartCoroutine(DoCameraMoveAnimation(targetPosition, targetFOV, resetSmoothSpeed)); // Use reset speed

        // Set the target for when the animation finishes and LateUpdate resumes following
        target = initialTarget;
        _baseBlockPlatformRef = initialTarget; // Update base ref
        Debug.Log("Camera Reset Animation Started.");
    }

    // Need access to GameManager's state (could pass it in or use Singleton)
    private GameState CurrentState => GameManager.Instance != null ?GameManager.Instance.GetCurrentState() : GameState.Starting; // Example access
                                                                                                     // Need access to base platform (could pass it in or use Singleton)
    private Transform BaseBlockPlatform => GameManager.Instance != null ? GameManager.Instance.baseBlockPlatform : null; // Example access
}