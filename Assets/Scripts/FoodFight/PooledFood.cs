using UnityEngine;
using UnityEngine.Pool;

public class PooledFood : MonoBehaviour
{
    private IObjectPool<PooledFood> _pool;
    private Rigidbody _rb;
    private bool _hasHit; // Prevents the double-release error

    void Awake() => _rb = GetComponent<Rigidbody>();

    public void SetPool(IObjectPool<PooledFood> pool)
    {
        _pool = pool;
        _hasHit = false;
        
        // Reset physics completely when "spawned"
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        _hasHit = true;

        // Splat logic placeholder (We will add pooled particles here later)
        
        // Return to pool
        _pool.Release(this);
    }
}