using UnityEngine;
using UnityEngine.Pool;

public class PooledFood : MonoBehaviour
{
    private IObjectPool<PooledFood> _pool;
    private Rigidbody _rb;
    private bool _hasHit;

    [Header("Spawning Settings")]
    public float spawnYOffset = 0.5f; 
    [Range(1, 10)] public int spawnChance = 6; 

    [Header("Visual Effects")]
    // Drag your specific splat particle prefab here in the Inspector
    public GameObject hitParticlePrefab; 

    [Header("Mess Feature")] 
    public GameObject messPrefab; 

    void Awake() => _rb = GetComponent<Rigidbody>();

    public void SetPool(IObjectPool<PooledFood> pool) 
    {
        _pool = pool;
        _hasHit = false;
        
        if (_rb != null) 
        { 
            _rb.velocity = Vector3.zero; 
            _rb.angularVelocity = Vector3.zero; 
        }
    }

    private void OnCollisionEnter(Collision collision) 
    {
        if (_hasHit) return;
        _hasHit = true;

        // 1. Play Particles
        if (hitParticlePrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            // Spawn at hit point, rotated to face out from the surface
            GameObject effect = Instantiate(hitParticlePrefab, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(effect, 2f); 
        }

        // 2. Handle Logic
        if (collision.gameObject.CompareTag("Principal")) 
        {
            if (PrincipalMinigame.instance != null) PrincipalMinigame.instance.GetHit();
        }
        else if (collision.gameObject.CompareTag("Floor")) 
        {
            if (Random.Range(1, spawnChance + 1) == 1) 
            {
                SpawnMess(collision.contacts[0].point);
            }
        }

        // 3. Return to Pool
        if (_pool != null) _pool.Release(this);
    }

    public void SpawnMess(Vector3 hitPosition)
    {
        // Faster check using the static instance
        if (PrincipalMinigame.instance == null || !PrincipalMinigame.instance.IsGameActive()) return;

        if (PrincipalMinigame.instance.CanSpawnMessAt(hitPosition))
        {
            if (messPrefab != null)
            {
                Vector3 spawnPos = hitPosition + (Vector3.up * spawnYOffset);
                GameObject newMess = Instantiate(messPrefab, spawnPos, Quaternion.Euler(-90, 0, 0));
                
                int layerIndex = LayerMask.NameToLayer("Interactions");
                if (layerIndex != -1) newMess.layer = layerIndex;
        
                PrincipalMinigame.instance.RegisterMess(newMess);
            }
        }
    }
}