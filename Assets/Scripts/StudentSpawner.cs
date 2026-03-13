using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentSpawner : MonoBehaviour
{
    public GameObject studentPrefab;

    public Transform[] spawnPoints;

    [Header("Students per Level")]
    public int[] studentsPerLevel;

    public int currentLevel = 1;

    void Start()
    {
        currentLevel = SinglePlayerModeManager.Instance.level;
        SpawnStudentsForLevel(currentLevel);
    }

    public void SpawnStudentsForLevel(int level)
    {
        if (level <= 0)
        {
            Debug.LogWarning("Level out of range!");
            return;
        }
        if(level > studentsPerLevel.Length)
        {
            level = studentsPerLevel.Length;
        }

        int studentsToSpawn = studentsPerLevel[level - 1];

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < studentsToSpawn; i++)
        {
            if (availableSpawns.Count == 0)
                break;

            int index = Random.Range(0, availableSpawns.Count);

            Transform spawnPoint = availableSpawns[index];

            Instantiate(studentPrefab, spawnPoint.position, spawnPoint.rotation);

            availableSpawns.RemoveAt(index);
        }
    }
}
