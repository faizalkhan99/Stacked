using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlockFollowClaw : MonoBehaviour
{
    private Transform _targetClaw;
    private Rigidbody _rb;
    private Vector3 _offset;
    private bool _isFollowing = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
    }

    // Called by GameManager to "attach" this block to the claw
    public void Initialize(Transform target, float verticalOffset)
    {
        _targetClaw = target;
        // The offset is how far below the claw's center the block should hang
        _offset = Vector3.up * verticalOffset;
        _isFollowing = true;
    }

    void LateUpdate()
    {
        // While attached, this block's position is locked to the claw
        if (_isFollowing && _targetClaw != null)
        {
            transform.position = _targetClaw.position - _offset;
        }
    }

    // Called by GameManager to "detach" this block
    public void Release()
    {
        _isFollowing = false;
        _rb.isKinematic = false;
        _rb.useGravity = true;

        // This component has done its job, remove it.
        Destroy(this);
    }
}