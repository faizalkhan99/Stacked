using UnityEngine;

public class BlockCutter : MonoBehaviour
{
    // The force is now passed in as an argument from the GameManager.
    public bool Cut(Transform fallingBlock, Transform stationaryBlock, float forceToApply)
    {
        // Calculate overlap
        float diffX = fallingBlock.position.x - stationaryBlock.position.x;
        float diffZ = fallingBlock.position.z - stationaryBlock.position.z;
        float newSizeX = stationaryBlock.localScale.x - Mathf.Abs(diffX);
        float newSizeZ = stationaryBlock.localScale.z - Mathf.Abs(diffZ);

        if (newSizeX <= 0 || newSizeZ <= 0)
        {
            Destroy(fallingBlock.gameObject);
            return false; // Game Over
        }

        // Calculate properties of the new placed block
        Vector3 placedBlockScale = new Vector3(newSizeX, fallingBlock.localScale.y, newSizeZ);
        float newX = stationaryBlock.position.x + (diffX / 2);
        float newZ = stationaryBlock.position.z + (diffZ / 2);
        Vector3 placedBlockPosition = new Vector3(newX, fallingBlock.position.y, newZ);

        // Spawn debris first, passing the force value to it.
        SpawnDebris(fallingBlock, placedBlockScale, placedBlockPosition, stationaryBlock.position, forceToApply);

        // Modify the falling block to become the new placed block.
        fallingBlock.transform.position = placedBlockPosition;
        fallingBlock.transform.localScale = placedBlockScale;

        // --- THIS IS THE FIX ---
        // Reset the block's rotation to be perfectly flat.
        // Quaternion.identity means "no rotation".
        fallingBlock.transform.rotation = Quaternion.identity;
        // ---

        return true; // Success!
    }

    private void SpawnDebris(Transform originalBlock, Vector3 placedScale, Vector3 placedPosition, Vector3 towerCenter, float force)
    {
        Vector3 originalScale = originalBlock.localScale;
        
        // Debris for X-axis cut
        if (originalScale.x > placedScale.x + 0.01f)
        {
            float debrisSizeX = originalScale.x - placedScale.x;
            Vector3 debrisScale = new Vector3(debrisSizeX, originalScale.y, placedScale.z);
            float direction = Mathf.Sign(originalBlock.position.x - placedPosition.x);
            Vector3 debrisPos = new Vector3(
                placedPosition.x + (placedScale.x / 2 + debrisSizeX / 2) * direction,
                originalBlock.position.y,
                placedPosition.z
            );
            SpawnSingleDebrisPiece(debrisPos, debrisScale, originalBlock.GetComponent<Renderer>().material, towerCenter, force);
        }

        // Debris for Z-axis cut
        if (originalScale.z > placedScale.z + 0.01f)
        {
            float debrisSizeZ = originalScale.z - placedScale.z;
            Vector3 debrisScale = new Vector3(originalScale.x, originalScale.y, debrisSizeZ);
            float direction = Mathf.Sign(originalBlock.position.z - placedPosition.z);
            Vector3 debrisPos = new Vector3(
                originalBlock.position.x,
                originalBlock.position.y,
                placedPosition.z + (placedScale.z / 2 + debrisSizeZ / 2) * direction
            );
            SpawnSingleDebrisPiece(debrisPos, debrisScale, originalBlock.GetComponent<Renderer>().material, towerCenter, force);
        }
    }

    private void SpawnSingleDebrisPiece(Vector3 position, Vector3 scale, Material material, Vector3 towerCenter, float force)
    {
        GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debris.transform.position = position;
        debris.transform.localScale = scale;
        //debris.GetComponent<Renderer>().material = material;
        
        Rigidbody rb = debris.AddComponent<Rigidbody>();
        
        // Calculate the direction away from the tower's center.
        Vector3 direction = (position - towerCenter).normalized;
        
        // Apply a direct force to THIS rigidbody ONLY.
        rb.AddForce(direction * force, ForceMode.Impulse);

        Destroy(debris, 3f);
    }
}