using System.Collections.Generic;
using UnityEngine;

public class DebrisPooler : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static DebrisPooler Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("The prefab for the debris (must have Rigidbody and DebrisFader scripts).")]
    [SerializeField] private GameObject debrisPrefab;
    [Tooltip("The initial number of debris objects to create.")]
    [SerializeField] private int initialPoolSize = 20;

    // The pool
    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        // Enforce Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializePool();
    }

    // Create the initial set of objects
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject debris = Instantiate(debrisPrefab);
            debris.transform.SetParent(this.transform); // Keep hierarchy clean
            debris.SetActive(false); // Deactivate until needed
            pool.Enqueue(debris);
        }
    }

    // Get an object from the pool
    public GameObject GetDebris()
    {
        if (pool.Count == 0)
        {
            // Pool is empty, create a new one (robust fallback)
            Debug.LogWarning("DebrisPooler: Pool is empty. Instantiating a new debris object.");
            GameObject newDebris = Instantiate(debrisPrefab);
            newDebris.transform.SetParent(this.transform);
            return newDebris; // Return it directly (it will be returned to pool later)
        }

        // Get an object from the pool
        GameObject debris = pool.Dequeue();
        debris.SetActive(true); // Activate it
        return debris;
    }

    // Return an object to the pool
    public void ReturnDebris(GameObject debris)
    {
        if (debris == null) return;
        
        debris.SetActive(false);
        // Ensure it's parented to the pooler for a clean hierarchy
        debris.transform.SetParent(this.transform); 
        pool.Enqueue(debris);
    }
}