using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtSpawnHolder : MonoBehaviour
{
    public static DirtSpawnHolder instance;
    public DirtSpawn[] dirtSpawns;

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
        
        // Automatically find all DirtSpawn children if not assigned manually
        if (dirtSpawns == null || dirtSpawns.Length == 0)
        {
            dirtSpawns = GetComponentsInChildren<DirtSpawn>();
        }
    }

    public DirtSpawn GetRandomDirtSpawn()
    {
        List<DirtSpawn> availableSpawns = new List<DirtSpawn>();

        foreach (DirtSpawn spawn in dirtSpawns)
        {
            if (!spawn.isSpawned)
            {
                availableSpawns.Add(spawn);
            }
        }

        if (availableSpawns.Count == 0)
            return null;

        DirtSpawn chosen = availableSpawns[Random.Range(0, availableSpawns.Count)];
        chosen.isSpawned = true; // Mark it as occupied
        return chosen;
    }
}