using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFacePlayer : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Make the UI look at the player
        transform.LookAt(player);

        // Rotate 180° around the Y axis so it's not flipped
       //not applicable here; transform.Rotate(0f, 180f, 0f);
    }
}
