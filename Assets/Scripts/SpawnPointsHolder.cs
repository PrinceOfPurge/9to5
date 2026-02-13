using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsHolder : MonoBehaviour
{
    [SerializeField]
    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public void SpawnRandom(int amount)
    {
        if (spawnPoints.Count == 0)
            return;

        amount = Mathf.Min(amount, spawnPoints.Count);

        List<SpawnPoint> shuffled = new List<SpawnPoint>(spawnPoints);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }

        for (int i = 0; i < amount; i++)
        {
            shuffled[i].Spawn();
        }
    }
}
