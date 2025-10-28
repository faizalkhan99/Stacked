// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class BlockSwinger : MonoBehaviour
// {
//     private float _speed;
//     private float _swingDistance;
//     private float _ropeLength;
//     private Vector3 _swingAxis;
//     private Vector3 _pivotPoint;
//     private bool _isSwinging = false;

//     // We'll need a Rigidbody for the drop, so let's get a reference to it.
//     private Rigidbody _rb;
//  private LineRenderer _lineRenderer; // Reference to our rope

//     void Awake()
//     {
//         _rb = GetComponent<Rigidbody>();
//         _rb.isKinematic = true;
//         _rb.useGravity = false;

//         // --- THE LINE RENDERER SETUP ---
//         _lineRenderer = gameObject.AddComponent<LineRenderer>();
//         _lineRenderer.positionCount = 2; // A rope is a line with a start and end point.
//         _lineRenderer.startWidth = 0.05f;
//         _lineRenderer.endWidth = 0.05f;
//         // You need a simple material for the line to be visible.
//         _lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
//         _lineRenderer.startColor = Color.white;
//         _lineRenderer.endColor = Color.white;
//     }

//     public void Initialize(float speed, float swingDistance, float ropeLength, Vector3 swingAxis, Vector3 pivotPoint)
//     {
//         _speed = speed;
//         _swingDistance = swingDistance;
//         _ropeLength = ropeLength;
//         _swingAxis = swingAxis.normalized;
//         _pivotPoint = pivotPoint;
//         _isSwinging = true;
//     }

//     void Update()
//     {
//         if (!_isSwinging) return;

//         // Use Mathf.Sin to get a smooth value between -1 and 1 for the swing.
//         float swingValue = Mathf.Sin(Time.time * _speed) * _swingDistance;

//         // The horizontal position is the pivot point offset by the swing value.
//         Vector3 horizontalPosition = _pivotPoint + (_swingAxis * swingValue);

//         // Use Mathf.Cos to make the block rise slightly at the edges of its swing, just like a real pendulum.
//         float verticalOffset = Mathf.Cos(Time.time * _speed) * _swingDistance;
        
//         // We calculate the final position.
//         // The Y position is the pivot's height minus the rope length, adjusted by the vertical offset.
//         // To make the effect subtle, let's make sure the block never goes above its lowest point.
//         float yPos = _pivotPoint.y - Mathf.Sqrt(Mathf.Pow(_ropeLength, 2) - Mathf.Pow(swingValue, 2));

//         transform.position = new Vector3(horizontalPosition.x, yPos, horizontalPosition.z);

//         _lineRenderer.SetPosition(0, _pivotPoint);      // Start of the rope
//         _lineRenderer.SetPosition(1, transform.position); // End of the rope
//     }

//     // This public method will be called by the GameManager to drop the block.
//     public void Release()
//     {
//         _isSwinging = false;
//         // Switch control from our script to Unity's physics engine.
//         _rb.isKinematic = false;
//         _rb.useGravity = true;
//         // Hide the rope when the block is dropped.
//         _lineRenderer.enabled = false;
//     }
// }


using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlockSwinger : MonoBehaviour
{
    private float _speed;
    private float _swingDistance;
    private float _ropeLength; // This is still used for the swing calculation
    private Vector3 _swingAxis;
    private Vector3 _pivotPoint;
    private bool _isSwinging = false;

    private Rigidbody _rb;
    private float _startTime;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        
        // All LineRenderer/visual code has been removed.
    }

    public void Initialize(float speed, float swingDistance, float ropeLength, Vector3 swingAxis, Vector3 pivotPoint)
    {
        _speed = speed;
        _swingDistance = swingDistance;
        _ropeLength = ropeLength;
        _swingAxis = swingAxis.normalized;
        _pivotPoint = pivotPoint;
        _isSwinging = true;
        _startTime = Time.time;
    }

    void Update()
    {
        if (!_isSwinging) return;

        // --- THIS IS THE FIX FOR THE "HORRIBLE SWING" ---
        // We now use Mathf.Sin() instead of PingPong.
        // This creates a smooth pendulum motion (fast at the center, slow at the ends).
        float timeElapsed = (Time.time - _startTime) * _speed;
        float pathPosition = Mathf.Sin(timeElapsed) * _swingDistance;
        // ---

        // This calculation for the 'Y' position already creates a natural arc.
        float yPos = _pivotPoint.y - Mathf.Sqrt(Mathf.Pow(_ropeLength, 2) - Mathf.Pow(pathPosition, 2));

        transform.position = _pivotPoint + (_swingAxis * pathPosition);
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
    }

    public void Release()
    {
        _isSwinging = false;
        _rb.isKinematic = false;
        _rb.useGravity = true;
    }
}