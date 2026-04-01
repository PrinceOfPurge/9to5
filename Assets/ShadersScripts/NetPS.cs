using UnityEngine;

public class NetPS : MonoBehaviour
{
    [Header("Scoring Effects")]
    [SerializeField] private ParticleSystem hoopParticles;

    private void Awake()
    {
        // Auto‑assign if not manually set
        if (hoopParticles == null)
            hoopParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Garbage"))
            return;

        PlayParticles();
        PlayAudio();
        NotifyPASystem();
    }

    private void PlayParticles()
    {
        if (hoopParticles == null)
            return;

        // Ensures the PS restarts every time
        hoopParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        hoopParticles.Play();
    }

    private void PlayAudio()
    {
        AudioManager.instance.PlayOneShot(
            FMODEvents.instance.Swish,
            transform.position
        );
    }

    private void NotifyPASystem()
    {
        PASystem pa = FindFirstObjectByType<PASystem>();
        if (pa != null)
            pa.CheckForInstantUpdate();
    }
}