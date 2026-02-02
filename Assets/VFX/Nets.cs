using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nets : MonoBehaviour
{
    private ParticleSystem hoopParticles;

    void Start()
    {
        hoopParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Garbage"))
        {
            if (hoopParticles != null)
                hoopParticles.Play();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Swish, transform.position);
        }
    }
}
