using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StudentAI : MonoBehaviour
{
    NavMeshAgent agent;

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
        if (!agent.pathPending && agent.remainingDistance < 1f && !makingMess)
        {
            if (returningHome)
            {
                agent.isStopped = true;
                return;
            }

            if (currentSpawn != null)
            {
                makingMess = true;
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

        Debug.Log("Student made a mess!");

        visitedSpawns.Add(currentSpawn);

        currentSpawn.isSpawned = false;
        currentSpawn = null;

        MoveToNextSpawn();
    }

    void ReturnHome()
    {
        returningHome = true;
        agent.SetDestination(homePosition);
    }
}
