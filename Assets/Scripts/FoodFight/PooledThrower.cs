using UnityEngine;

public class PooledThrower : MonoBehaviour
{
    public Transform handTransform;
    public float throwForce = 15f;

    [Header("Targeting")]
    public Transform targetOverride; 
    public bool lookAtTarget = true;

    [Header("Chaos Timing")]
    public float minDelay = 2f;    
    public float maxDelay = 5f;

    private Quaternion _initialRotation;
    private Animator _anim;
    private bool _isStopped = false; // NEW: Local flag to kill the loop

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _initialRotation = transform.rotation;
    }

    void Start()
    {
        // Start throwing immediately when the scene loads
        Invoke("StartThrowCycle", Random.Range(0.5f, 2f));
    }

    void StartThrowCycle()
    {
        if (_isStopped) return; // Stop forever if the player won

        float nextDelay = Random.Range(minDelay, maxDelay);

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
        if (_isStopped) return; // Double check in case win happened mid-animation

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

    // This is called by the Principal when the player wins
    public void StopThrowingPermanently()
    {
        _isStopped = true;
        CancelInvoke("StartThrowCycle");
        if (_anim != null) _anim.Play("Act_StudentIdleAnim"); 
    }
}