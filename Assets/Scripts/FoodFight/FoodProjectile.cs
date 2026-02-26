using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodProjectile : MonoBehaviour
{
    [Header("Impact Settings")]
    public GameObject splatterPrefab; // Drag your Particle System prefab here
    public float destroyDelay = 0.1f;

    [Header("Visuals")]
    public float rotationSpeed = 500f;
    private Vector3 randomRotationAxis;

    void Start()
    {
        // Give the food a random "tumble" for realism
        randomRotationAxis = new Vector3(Random.value, Random.value, Random.value);
        
        // Safety: Destroy the object after 10 seconds if it hits nothing
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        // Spin the object as it flies
        transform.Rotate(randomRotationAxis * rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Get the point where the food hit the surface
        ContactPoint contact = collision.contacts[0];
        
        // 2. Spawn the splatter at that exact point, rotated to match the surface
        if (splatterPrefab != null)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Instantiate(splatterPrefab, contact.point, rot);
        }

        // 3. Remove the food object
        Destroy(gameObject, destroyDelay);
    }
}