using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles spawning of pooled visual FX (only for perfect hits).
/// No audio logic. Uses simple object pooling for performance.
/// </summary>
public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    [Header("Perfect Impact Prefab")]
    [Tooltip("Prefab that contains the dust + spark ring particle effects.")]
    [SerializeField] private GameObject perfectImpactPrefab;

    [Header("Pooling Settings")]
    [Tooltip("Initial number of pooled FX instances.")]
    [SerializeField] private int poolSize = 8;

    private readonly Queue<GameObject> _fxPool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
        InitializePool();
    }

    private void InitializePool()
    {
        if (perfectImpactPrefab == null) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject fx = Instantiate(perfectImpactPrefab);
            fx.SetActive(false);
            _fxPool.Enqueue(fx);
        }
    }

    /// <summary>
    /// Spawns a perfect drop FX using pooled instance.
    /// </summary>
    public void PlayPerfectImpact(Vector3 previousBlockPos, Vector3 previousBlockScale)
    {
        if (_fxPool.Count == 0)
            ExpandPool();

        GameObject fx = _fxPool.Dequeue();

        // Calculate top-center of the previous block
        float topY = previousBlockPos.y + (previousBlockScale.y * 0.5f);
        Vector3 spawnPos = new Vector3(previousBlockPos.x, topY, previousBlockPos.z);

        // Apply small downward offset (optional, tweak if ring clips visually)
        spawnPos.y += 0.01f;

        fx.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        fx.SetActive(true);

        if (fx.TryGetComponent<PerfectImpactFX>(out var impact))
            impact.Initialize(previousBlockScale);

        StartCoroutine(ReturnToPoolAfterLifetime(fx, impact != null ? impact.Lifetime : 1f));
    }


    private void ExpandPool()
    {
        // Double pool size when needed
        int newSize = Mathf.Max(1, poolSize / 2);
        for (int i = 0; i < newSize; i++)
        {
            GameObject fx = Instantiate(perfectImpactPrefab);
            fx.SetActive(false);
            _fxPool.Enqueue(fx);
        }
    }

    private System.Collections.IEnumerator ReturnToPoolAfterLifetime(GameObject fx, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        fx.SetActive(false);
        _fxPool.Enqueue(fx);
    }
}
