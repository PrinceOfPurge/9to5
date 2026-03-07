using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessSpawnHolder : MonoBehaviour
{
    public static MessSpawnHolder instance;
    public MessSpawn[] messSpawns;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public MessSpawn GetRandomMessSpawn(List<MessSpawn> excludedSpawns)
    {
        List<MessSpawn> availableSpawns = new List<MessSpawn>();

        foreach (MessSpawn spawn in messSpawns)
        {
            if (!spawn.isSpawned && !excludedSpawns.Contains(spawn))
            {
                availableSpawns.Add(spawn);
            }
        }

        if (availableSpawns.Count == 0)
            return null;

        MessSpawn chosen = availableSpawns[Random.Range(0, availableSpawns.Count)];
        chosen.isSpawned = true;

        return chosen;
    }
}
