using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StudentSpawner : MonoBehaviour
{
    public GameObject studentPrefab;
    public Transform[] spawnPoints;

    [Header("Students per Level")]
    public int[] studentsPerLevel;

    private int currentLevel = 1;

    void Start()
    {
        if (SinglePlayerModeManager.Instance != null)
        {
            currentLevel = SinglePlayerModeManager.Instance.level;
            SpawnStudentsForLevel(currentLevel);
        }
    }

    public void SpawnStudentsForLevel(int level)
    {
        if (level <= 0) return;
        
        int dataIndex = Mathf.Min(level, studentsPerLevel.Length) - 1;
        int studentsToSpawn = studentsPerLevel[dataIndex];

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < studentsToSpawn; i++)
        {
            if (availableSpawns.Count == 0) break;

            int index = Random.Range(0, availableSpawns.Count);
            Transform spawnPoint = availableSpawns[index];

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPoint.position, out hit, 5f, NavMesh.AllAreas))
            {
                // Instantiate at the snapped NavMesh position
                Instantiate(studentPrefab, hit.position, spawnPoint.rotation);
                
                if (SinglePlayerModeManager.Instance != null)
                    SinglePlayerModeManager.Instance.ActiveStudents++;
            }

            availableSpawns.RemoveAt(index);
        }
    }
}