using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nets : MonoBehaviour
{
    public static Nets instance;
    public bool isWon { get; private set; } = false;

    private ParticleSystem hoopParticles;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hoopParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    void OnTriggerEnter(Collider other)
    {
        // If already won, don't trigger logic again
        if (isWon) return;

        if (other.CompareTag("Garbage"))
        {
            isWon = true; // Set win state for the PA System
            
            if (hoopParticles != null)
                hoopParticles.Play();
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Swish, transform.position);
            
            // Optional: Tell the PA system to re-check completion immediately
            if(PASystem.instance != null) PASystem.instance.CheckForInstantUpdate();
        }
    }
}