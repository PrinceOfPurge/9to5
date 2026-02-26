using UnityEngine;

public class PooledThrower : MonoBehaviour
{
    public Transform handTransform;
    public float throwForce = 15f;

    [Header("Randomization")]
    public float maxRotationAngle = 30f; 
    public float minDelay = 2f;    
    public float maxDelay = 5f;

    private Quaternion _initialRotation;
    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _initialRotation = transform.rotation;
    }

    void Start()
    {
        // Start the loop
        Invoke("StartThrowCycle", Random.Range(1f, 2f));
    }

    void StartThrowCycle()
    {
        // 1. Rotate to a new target
        float randomY = Random.Range(-maxRotationAngle, maxRotationAngle);
        transform.rotation = _initialRotation * Quaternion.Euler(0, randomY, 0);

        // 2. Play animation
        _anim.SetTrigger("tThrow");

        // 3. Repeat
        Invoke("StartThrowCycle", Random.Range(minDelay, maxDelay));
    }

    // THIS IS YOUR ANIMATION EVENT
    // Place this event at the "release" frame of your animation
    public void LaunchFood()
    {
        // 1. Get food from pool
        int randomType = Random.Range(0, 4); 
        PooledFood food = FoodPooler.Instance.GetFood(randomType);
        
        // 2. Position it at the hand
        food.transform.position = handTransform.position;
        food.transform.rotation = handTransform.rotation;

        // 3. Launch it using the NPC's forward direction
        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = transform.forward + (Vector3.up * 0.2f);
            rb.AddForce(direction * throwForce, ForceMode.Impulse);
        }
    }
}