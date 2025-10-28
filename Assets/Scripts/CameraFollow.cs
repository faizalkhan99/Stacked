using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("The object the camera will follow.")]
    public Transform target;
    [Tooltip("How quickly the camera catches up to the target.")]
    public float smoothSpeed = 0.125f;

    private Vector3 _offset;
    private Vector3 _initialPosition; // To store the starting X and Z

    void Start()
    {
        _initialPosition = transform.position;
        if (target != null)
        {
            _offset = transform.position - target.position;
        }
        Debug.Log($"CameraFollow Start: Target='{target?.name}', TargetPos={target?.position}, CameraPos={transform.position}, Calculated Offset={_offset}");
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- THE FIX IS HERE ---
        // 1. Calculate the desired vertical (Y) position only.
        float desiredY = target.position.y + _offset.y;

        // 2. Smoothly interpolate only the Y position.
        float smoothedY = Mathf.Lerp(transform.position.y, desiredY, smoothSpeed);

        // 3. Create the new camera position using the original X and Z, but the new Y.
        transform.position = new Vector3(_initialPosition.x, smoothedY, _initialPosition.z);
        Debug.Log($"CameraFollow LateUpdate: TargetPos={target.position}, Offset={_offset}, DesiredY={desiredY}, SmoothedY={smoothedY}, NewCamPos={transform.position}");
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}