using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodThrower : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] foodPrefabs; // List of your food items
    public Transform handTransform; // Create an Empty GO in the NPC's hand and drag it here
    
    [Header("Physics")]
    public float throwForce = 15f;
    public float upwardArc = 2f; // Helps the food lob slightly

    // This function MUST be called exactly "LaunchFood" in your Animation Event
    public void LaunchFood()
    {
        if (foodPrefabs.Length == 0 || handTransform == null) return;

        // 1. Pick a random piece of food
        int randomIndex = Random.Range(0, foodPrefabs.Length);
        
        // 2. Create the food at the hand's position
        GameObject projectile = Instantiate(foodPrefabs[randomIndex], handTransform.position, handTransform.rotation);
        
        // 3. Apply the force
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Calculate direction: Hand's forward direction + a slight upward lift
            Vector3 forceDir = handTransform.forward * throwForce + Vector3.up * upwardArc;
            rb.AddForce(forceDir, ForceMode.Impulse);
        }
    }
}
