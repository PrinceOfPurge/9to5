using UnityEngine;

public class PooledThrower : MonoBehaviour
{
    public Transform handTransform;
    public float throwForce = 15f;

    [Header("Targeting")]
    public Transform targetOverride; 
    public bool lookAtTarget = true;

    [Header("Chaos Timing")]
    public float minDelay = 1f;    
    public float maxDelay = 3f;

    [Header("Balance Settings")]
    [Tooltip("Minimum time (in seconds) that MUST pass after ANY student throws before another can throw.")]
    public float baseGlobalCooldown = 0.6f; 
    
    private static float _globalLastThrowTime = 0f;

    private Quaternion _initialRotation;
    private Animator _anim;
    private bool _isStopped = false; 

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _initialRotation = transform.rotation;
    }

    void Start()
    {
        // Initial delay scaled
        float startDelay = Random.Range(0.5f, 2.5f) / PrincipalMinigame.ThrowSpeedMultiplier;
        Invoke("StartThrowCycle", startDelay);
    }

    void StartThrowCycle()
    {
        if (_isStopped) return; 

        // At level 1, this is 0.6. At level 4, this drops near 0.
        // This allows MULTIPLE kids to throw simultaneously at high levels (Bursts!)
        float currentGlobalCooldown = Mathf.Max(0.05f, baseGlobalCooldown - (PrincipalMinigame.DifficultyLevel * 0.15f));

        if (Time.time - _globalLastThrowTime < currentGlobalCooldown)
        {
            // Keep checking frequently so they don't lose their turn
            Invoke("StartThrowCycle", Random.Range(0.05f, 0.15f));
            return;
        }

        _globalLastThrowTime = Time.time;

        // Tighten the random range at higher levels so throws are consistently relentless
        float currentMaxDelay = Mathf.Max(minDelay + 0.2f, maxDelay / PrincipalMinigame.ThrowSpeedMultiplier);
        float currentMinDelay = minDelay / PrincipalMinigame.ThrowSpeedMultiplier;
        
        float nextDelay = Random.Range(currentMinDelay, currentMaxDelay);

        if (targetOverride != null)
        {
            Vector3 targetDir = targetOverride.position - transform.position;
            targetDir.y = 0; 
            if (lookAtTarget) transform.rotation = Quaternion.LookRotation(targetDir);
        }
        else
        {
            float randomY = Random.Range(-30f, 30f);
            transform.rotation = _initialRotation * Quaternion.Euler(0, randomY, 0);
        }

        if (_anim != null) _anim.SetTrigger("tThrow");

        Invoke("StartThrowCycle", nextDelay);
    }

    public void LaunchFood()
    {
        if (_isStopped) return; 

        PooledFood food = FoodPooler.Instance.GetFood(Random.Range(0, 4));
        if (food == null) return;

        food.transform.position = handTransform.position;
        food.transform.rotation = handTransform.rotation;

        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 direction = (targetOverride != null) 
                ? (targetOverride.position + Vector3.up * 1.2f - handTransform.position).normalized 
                : (transform.forward + (Vector3.up * 0.2f));

            rb.AddForce(direction * throwForce, ForceMode.Impulse);
        }
    }

    public void StopThrowingPermanently()
    {
        _isStopped = true;
        CancelInvoke("StartThrowCycle");
        if (_anim != null) _anim.Play("Act_StudentIdleAnim"); 
    }
}