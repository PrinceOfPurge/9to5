using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField]
    private GameObject[] possibleTrashPrefabs;

    public void Spawn()
    {
        if (possibleTrashPrefabs == null || possibleTrashPrefabs.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, possibleTrashPrefabs.Length);

        Instantiate(
            possibleTrashPrefabs[index],
            transform.position,
            transform.rotation
        );
    }
}
