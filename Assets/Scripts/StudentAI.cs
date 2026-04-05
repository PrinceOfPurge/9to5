using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StudentAI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator; 

    public float moveSpeed = 3.5f;

    [Header("Difficulty Control")]
    public bool isHarmless = false;

    [Header("Garbage/Banana Messes")]
    public int minMessessToSpawn = 1;
    public int maxMessessToSpawn = 4;
    public GameObject messPrefab;

    [Header("Dirty Footstep Messes")]
    public GameObject dirtPrefab; 
    public int maxDirtSpawns = 2;              

    [Header("Endless Wandering Settings")]
    public float wanderRadius = 15f;
    public float wanderWaitTime = 4f;

    [Header("Collision Settings")]
    public float fallRecoveryTime = 3.0f;

    private int currentDirtSpawns = 0;
    int messesRemainingToSpawn;

    MonoBehaviour currentTargetPoint;

    bool makingMess = false;
    bool isWanderingForever = false;
    bool isWaitingToWander = false;
    bool isFalling = false; 

    private float stuckTimer = 0f;

    List<MonoBehaviour> visitedSpawns = new List<MonoBehaviour>();

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>(); 

        if (agent != null) 
        {
            agent.speed = moveSpeed;
            agent.avoidancePriority = Random.Range(10, 90); 
        }
        
        if (isHarmless)
        {
            messesRemainingToSpawn = 0;
            currentDirtSpawns = maxDirtSpawns; 
        }
        else
        {
            messesRemainingToSpawn = Random.Range(minMessessToSpawn, maxMessessToSpawn + 1);
        }

        // The "Middle Ground": A tiny random jitter (0 to 0.5 seconds)
        // Spreads the CPU pathfinding load and makes their starts feel natural.
        Invoke("MoveToNextSpawn", Random.Range(0f, 0.5f));
    }

    private void Update()
    {
        if (isFalling) return;

        bool isMoving = agent != null && agent.enabled && agent.velocity.sqrMagnitude > 0.01f; 
        if (animator != null) animator.SetBool("IsRunning", isMoving);

        if (isWanderingForever)
        {
            if (agent != null && agent.enabled && !agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance && !isWaitingToWander)
                {
                    stuckTimer = 0f; 
                    StartCoroutine(WaitThenWander());
                }
                else if (!isWaitingToWander)
                {
                    if (agent.velocity.sqrMagnitude < 0.1f)
                    {
                        stuckTimer += Time.deltaTime;
                        if (stuckTimer > 2.0f) 
                        {
                            stuckTimer = 0f;
                            PickNewWanderDestination(); 
                        }
                    }
                    else
                    {
                        stuckTimer = 0f; 
                    }
                }
            }
        }
        else if (agent != null && agent.enabled && !agent.pathPending && !makingMess)
        {
            if (agent.remainingDistance <= agent.stoppingDistance && currentTargetPoint != null)
            {
                makingMess = true;
                if (animator != null) animator.SetBool("IsRunning", false);
                
                if (currentTargetPoint is DirtSpawn) SpawnDirt();
                else MakeMess();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFalling)
        {
            StartCoroutine(KnockdownRoutine());
        }
    }

    private IEnumerator KnockdownRoutine()
    {
        isFalling = true;

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetBool("IsRunning", false); 
            animator.SetTrigger("Fall");
        }

        yield return new WaitForSeconds(fallRecoveryTime);

        if (animator != null)
        {
            animator.ResetTrigger("Fall"); 
        }

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
        }

        stuckTimer = 0f; 
        isFalling = false;
    }

    void SpawnDirt()
    {
        DirtSpawn dp = currentTargetPoint as DirtSpawn;
        if (dp != null && dirtPrefab != null)
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
        if (ms != null && messPrefab != null)
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
        if (messesRemainingToSpawn <= 0 && currentDirtSpawns >= maxDirtSpawns)
        {
            StartWanderingForever();
            return;
        }

        makingMess = false;

        if (currentDirtSpawns < maxDirtSpawns && (Random.value > 0.5f || messesRemainingToSpawn <= 0))
        {
            if (DirtSpawnHolder.instance != null) 
                currentTargetPoint = DirtSpawnHolder.instance.GetRandomDirtSpawn();
        }
        else if (messesRemainingToSpawn > 0)
        {
            List<MessSpawn> excluded = new List<MessSpawn>();
            foreach(var v in visitedSpawns) if(v is MessSpawn) excluded.Add(v as MessSpawn);
            
            if (MessSpawnHolder.instance != null) 
                currentTargetPoint = MessSpawnHolder.instance.GetRandomMessSpawn(excluded);
        }

        if (currentTargetPoint != null && agent != null && agent.enabled)
        {
            agent.SetDestination(currentTargetPoint.transform.position);
        }
        else
        {
            StartWanderingForever(); 
        }
    }

    void StartWanderingForever()
    {
        isWanderingForever = true;
        PickNewWanderDestination();
    }

    void PickNewWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            if (agent != null && agent.enabled)
            {
                agent.SetDestination(hit.position);
                stuckTimer = 0f; 
            }
        }
    }

    IEnumerator WaitThenWander()
    {
        isWaitingToWander = true;
        if (animator != null) animator.SetBool("IsRunning", false);
        
        yield return new WaitForSeconds(Random.Range(1f, wanderWaitTime));
        
        PickNewWanderDestination();
        isWaitingToWander = false;
    }
}