using UnityEngine;

public class DroppedBlockDetector : MonoBehaviour
{
    public GameManager gameManager;
    private bool _hasCollided = false;

    void OnCollisionEnter(Collision collision)
    {
        // Prevent this from running more than once.
        if (_hasCollided) return;

        // --- THIS IS THE NEW LOGIC ---

        // Case 1: Successful hit on the target block.
        if (collision.gameObject.CompareTag("TopBlock"))
        {
            _hasCollided = true;

            // Freeze the block instantly for the cut.
            GetComponent<Rigidbody>().isKinematic = true;

            // Tell the GameManager to process the successful placement.
            gameManager.ProcessLandedBlock(transform);
        }
        // Case 2: Missed the target but hit an older block.
        else if (collision.gameObject.CompareTag("Block"))
        {
            _hasCollided = true;

            // DO NOT make it kinematic. Let the physics engine handle the bounce/roll.
            
            // Tell the GameManager it was a miss.
            gameManager.HandleMissedPlacement(transform);
        }
    }
}