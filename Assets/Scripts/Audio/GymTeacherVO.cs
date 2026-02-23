using UnityEngine;
using FMODUnity;

public class GymTeacherVO : MonoBehaviour
{
    [SerializeField] private string garbageTag = "Garbage";
    [SerializeField] private float hitCooldown = 0.1f; // just to debounce collisions
    [SerializeField] private float hitSfxDuration = 1.5f; // approx length of VO

    private float lastHitTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(garbageTag)) return;
        if (Time.time < lastHitTime + hitCooldown) return;
        if (!NPCVoiceManager.instance.CanPlay()) return;

        lastHitTime = Time.time;
        RuntimeManager.PlayOneShot(FMODEvents.instance.GymTeacherHit, transform.position);
        NPCVoiceManager.instance.PlayForDuration(hitSfxDuration);
    }

    // Called by GreetingTrigger or distance script
    public void TriggerGreeting()
    {
        if (!NPCVoiceManager.instance.CanPlay()) return;

        RuntimeManager.PlayOneShot(FMODEvents.instance.GymTeacherGreet, transform.position);
        NPCVoiceManager.instance.PlayForDuration(5.5f); // approximate length of greeting
    }
}