using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StudentAI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator; 

    public float moveSpeed = 3.5f;
    public float maxWaitTime = 25f;

    [Header("Garbage/Banana Messes")]
    public int minMessessToSpawn = 1;
    public int maxMessessToSpawn = 4;
    public GameObject messPrefab;

    [Header("Dirty Footstep Messes")]
    public GameObject dirtPrefab; 
    public int maxDirtSpawns = 2;              
    // Note: Timer logic removed because students now WALK to these spots

    private int currentDirtSpawns = 0;
    int messesRemainingToSpawn;

    Vector3 homePosition;
    Quaternion homeRotation; 
    
    // The current target can now be either a MessSpawn or a DirtSpawn
    MonoBehaviour currentTargetPoint;

    bool returningHome = false;
    bool makingMess = false;
    bool isExiting = false;

    List<MonoBehaviour> visitedSpawns = new List<MonoBehaviour>();

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>(); 

        if (agent != null) agent.speed = moveSpeed;
        
        homePosition = transform.position;
        homeRotation = transform.rotation; 

        messesRemainingToSpawn = Random.Range(minMessessToSpawn, maxMessessToSpawn + 1);

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        float delay = Random.Range(0f, maxWaitTime);
        yield return new WaitForSeconds(delay);
        MoveToNextSpawn();
    }

    private void Update()
    {
        if (isExiting) return;

        bool isMoving = agent != null && agent.enabled && agent.velocity.sqrMagnitude > 0.01f; 
        if (animator != null) animator.SetBool("IsRunning", isMoving);

        // --- Arrival Logic ---
        if (returningHome)
        {
            Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatHome = new Vector3(homePosition.x, 0, homePosition.z);
            float flatDist = Vector3.Distance(flatPos, flatHome);

            if (flatDist < 2.0f || (flatDist < 3.5f && !isMoving))
            {
                StartCoroutine(PerformClumsyExit());
            }
        }
        else if (agent != null && agent.enabled && !agent.pathPending && !makingMess)
        {
            if (agent.remainingDistance <= agent.stoppingDistance && currentTargetPoint != null)
            {
                makingMess = true;
                if (animator != null) animator.SetBool("IsRunning", false);
                
                // Determine if we are at a Dirt point or a Banana point
                if (currentTargetPoint is DirtSpawn) SpawnDirt();
                else MakeMess();
            }
        }
    }

    void SpawnDirt()
    {
        DirtSpawn dp = currentTargetPoint as DirtSpawn;
        if (dp != null)
        {
            currentDirtSpawns++;
            GameObject spawnedDirt = Instantiate(dirtPrefab, dp.transform.position, dirtPrefab.transform.rotation);
            
            DirtCleaner cleaner = spawnedDirt.GetComponent<DirtCleaner>();
            if (cleaner != null) cleaner.originSpawnPoint = dp;
            
            if (SinglePlayerModeManager.Instance != null)
                SinglePlayerModeManager.Instance.BagsRemaining++;
        }

        visitedSpawns.Add(currentTargetPoint);
        currentTargetPoint = null;
        MoveToNextSpawn();
    }

    void MakeMess()
    {
        MessSpawn ms = currentTargetPoint as MessSpawn;
        if (ms != null)
        {
            messesRemainingToSpawn--;
            Instantiate(messPrefab, ms.transform.position, Quaternion.identity);
            
            if (SinglePlayerModeManager.Instance != null)
                SinglePlayerModeManager.Instance.BagsRemaining++;
        }

        visitedSpawns.Add(currentTargetPoint);
        currentTargetPoint = null;
        MoveToNextSpawn();
    }

    void MoveToNextSpawn()
    {
        // If we finished banana messes AND dirt messes, go home
        if (messesRemainingToSpawn <= 0 && currentDirtSpawns >= maxDirtSpawns)
        {
            ReturnHome();
            return;
        }

        makingMess = false;

        // Decide whether to go to a Dirt spot or a Banana spot
        // If we still need dirt and (randomly chosen OR no banana messes left)
        if (currentDirtSpawns < maxDirtSpawns && (Random.value > 0.5f || messesRemainingToSpawn <= 0))
        {
            currentTargetPoint = DirtSpawnHolder.instance.GetRandomDirtSpawn();
        }
        else if (messesRemainingToSpawn > 0)
        {
            // Note: We need to cast our visited list to MessSpawn for the old holder logic
            List<MessSpawn> excluded = new List<MessSpawn>();
            foreach(var v in visitedSpawns) if(v is MessSpawn) excluded.Add(v as MessSpawn);
            
            currentTargetPoint = MessSpawnHolder.instance.GetRandomMessSpawn(excluded);
        }

        // Set destination
        if (currentTargetPoint != null && agent != null && agent.enabled)
        {
            agent.SetDestination(currentTargetPoint.transform.position);
        }
        else
        {
            // If we couldn't find a spot but still have "messes" to make, go home early
            ReturnHome();
        }
    }

    void ReturnHome()
    {
        returningHome = true;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.stoppingDistance = 0.5f; 
            agent.SetDestination(homePosition);
        }
    }

    IEnumerator PerformClumsyExit()
    {
        if (isExiting) yield break;
        isExiting = true;
        if (agent != null) agent.enabled = false;

        transform.position = homePosition + new Vector3(0, 1.2f, 0); 
        if (animator != null) animator.SetBool("IsRunning", false); 

        float turnDuration = 0.3f; 
        float elapsed = 0f;
        Quaternion currentRot = transform.rotation;
        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(currentRot, homeRotation, elapsed / turnDuration);
            yield return null;
        }
        transform.rotation = homeRotation; 

        if (animator != null) animator.SetTrigger("Fall"); 
        yield return new WaitForSeconds(3.0f);

        if (SinglePlayerModeManager.Instance != null)
            SinglePlayerModeManager.Instance.ActiveStudents--;

        Destroy(gameObject);
    }
}