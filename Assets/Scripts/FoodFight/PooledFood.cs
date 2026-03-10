using UnityEngine;
using UnityEngine.Pool;

public class PooledFood : MonoBehaviour
{
    private IObjectPool<PooledFood> _pool;
    private Rigidbody _rb;
    private bool _hasHit;

    [Header("Spawning Settings")]
    public float spawnYOffset = 1f; 
    [Range(1, 10)] public int spawnChance = 6; 

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

        if (collision.gameObject.CompareTag("Principal")) 
        {
            PrincipalMinigame principal = collision.gameObject.GetComponent<PrincipalMinigame>();
            if (principal != null) principal.GetHit();
        }
        else if (collision.gameObject.CompareTag("Floor")) 
        {
            if (Random.Range(1, spawnChance + 1) == 1) 
            {
                SpawnMess(collision.contacts[0].point);
            }
        }

        if (_pool != null) _pool.Release(this);
    }

    public void SpawnMess(Vector3 hitPosition)
    {
        PrincipalMinigame principal = FindObjectOfType<PrincipalMinigame>();
        
        // If no principal exists or game isn't running, don't spawn anything
        if (principal == null || !principal.IsGameActive()) return;

        // Check if the spot is within range and we haven't hit the max limit
        if (principal.CanSpawnMessAt(hitPosition))
        {
            if (messPrefab != null)
            {
                Vector3 spawnPos = hitPosition + (Vector3.up * spawnYOffset);
                Quaternion fixedRotation = Quaternion.Euler(-90, 0, 0);
        
                GameObject newMess = Instantiate(messPrefab, spawnPos, fixedRotation);
        
                // Set the layer so the Player's Raycast can detect it
                int layerIndex = LayerMask.NameToLayer("Interactions");
                if (layerIndex != -1) newMess.layer = layerIndex;
        
                // Register with the Principal so the UI counter updates
                principal.RegisterMess(newMess);
            }
        }
    }
}