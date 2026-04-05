using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StudentSpawner : MonoBehaviour
{
    [Header("Student Prefabs")]
    public GameObject messyStudentPrefab;
    public GameObject harmlessStudentPrefab;
    public Transform[] spawnPoints;

    [Header("Exact Counts Per Level")]
    [Tooltip("Level 1 is Element 0. How many MESSY students per level?")]
    public int[] messyStudentsPerLevel;
    
    [Tooltip("Level 1 is Element 0. How many HARMLESS students per level?")]
    public int[] harmlessStudentsPerLevel;

    [Header("Mayhem Flow")]
    public float minSpawnDelay = 1.5f; 
    public float maxSpawnDelay = 4.0f; 

    void Start()
    {
        if (SinglePlayerModeManager.Instance != null)
        {
            int currentLevel = SinglePlayerModeManager.Instance.level;
            SpawnStudentsForLevel(currentLevel);
        }
    }

    public void SpawnStudentsForLevel(int level)
    {
        StartCoroutine(SpawnStudentsForLevelRoutine(level));
    }

    private IEnumerator SpawnStudentsForLevelRoutine(int level)
    {
        if (level <= 0 || spawnPoints.Length == 0) yield break;
        
        // Find the right index for the current level (Level 1 = Index 0)
        int dataIndex = Mathf.Min(level, messyStudentsPerLevel.Length) - 1;
        
        // Get exact amounts
        int messyToSpawn = messyStudentsPerLevel[dataIndex];
        int harmlessToSpawn = 0;
        
        // Failsafe in case you forget to fill out the harmless array
        if (dataIndex < harmlessStudentsPerLevel.Length)
        {
            harmlessToSpawn = harmlessStudentsPerLevel[dataIndex];
        }

        // 1. Put all the requested students into a temporary list
        List<GameObject> studentsToSpawnList = new List<GameObject>();
        for (int i = 0; i < messyToSpawn; i++) studentsToSpawnList.Add(messyStudentPrefab);
        for (int i = 0; i < harmlessToSpawn; i++) studentsToSpawnList.Add(harmlessStudentPrefab);

        // 2. Shuffle the list! This makes them walk out of the doors in a mixed-up, random order
        for (int i = 0; i < studentsToSpawnList.Count; i++)
        {
            GameObject temp = studentsToSpawnList[i];
            int randomIndex = Random.Range(i, studentsToSpawnList.Count);
            studentsToSpawnList[i] = studentsToSpawnList[randomIndex];
            studentsToSpawnList[randomIndex] = temp;
        }

        // 3. Spawn them one by one with a delay
        foreach (GameObject prefabToSpawn in studentsToSpawnList)
        {
            int doorIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[doorIndex];

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPoint.position, out hit, 5f, NavMesh.AllAreas))
            {
                Instantiate(prefabToSpawn, hit.position, spawnPoint.rotation);
                
                if (SinglePlayerModeManager.Instance != null)
                    SinglePlayerModeManager.Instance.ActiveStudents++;
            }

            // Wait before the next student appears
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }
}