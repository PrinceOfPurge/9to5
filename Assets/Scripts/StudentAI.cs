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

    public int minMessessToSpawn = 1;
    public int maxMessessToSpawn = 4;

    public GameObject messPrefab;

    int messesRemainingToSpawn;

    Vector3 homePosition;
    MessSpawn currentSpawn;

    bool returningHome = false;
    bool makingMess = false;

    List<MessSpawn> visitedSpawns = new List<MessSpawn>();

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        animator = GetComponentInChildren<Animator>(); 

        agent.speed = moveSpeed;

        homePosition = transform.position;

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
        if (animator != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsRunning", isMoving);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !makingMess)
        {
            if (returningHome)
            {
                agent.isStopped = true;
                
                // stop animating when they reach home
                if (animator != null) animator.SetBool("IsRunning", false); 
                
                return;
            }

            if (currentSpawn != null)
            {
                makingMess = true;
                
                // stop animating while making a mess
                if (animator != null) animator.SetBool("IsRunning", false);
                
                MakeMess();
            }
        }
    }

    void MoveToNextSpawn()
    {
        if (messesRemainingToSpawn <= 0)
        {
            ReturnHome();
            return;
        }

        makingMess = false;

        currentSpawn = MessSpawnHolder.instance.GetRandomMessSpawn(visitedSpawns);

        if (currentSpawn != null)
        {
            agent.SetDestination(currentSpawn.transform.position);
        }
    }

    void MakeMess()
    {
        messesRemainingToSpawn--;

        Instantiate(messPrefab, currentSpawn.transform.position, Quaternion.identity);
        SinglePlayerModeManager.Instance.BagsRemaining++;

        Debug.Log("Student made a mess!");

        visitedSpawns.Add(currentSpawn);

        currentSpawn = null;

        MoveToNextSpawn();
    }

    void ReturnHome()
    {
        if (!returningHome) // Prevent this from triggering multiple times
        {
            returningHome = true;
            SinglePlayerModeManager.Instance.ActiveStudents--; // Tell the manager they are done!
            agent.SetDestination(homePosition);
        }
    }
}