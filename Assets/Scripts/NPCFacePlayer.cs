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

        // 1. Get the direction to the player
        Vector3 targetPostition = new Vector3(player.position.x, 
            this.transform.position.y, 
            player.position.z);

        // 2. Look at that flattened position
        this.transform.LookAt(targetPostition);
    }
}