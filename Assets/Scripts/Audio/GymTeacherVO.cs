using UnityEngine;
using FMODUnity;

public class GymTeacherVO : MonoBehaviour, IInteractable
{
    [Header("Minigame Settings")]
    [SerializeField] private string garbageTag = "Garbage";
    [SerializeField] private float hitCooldown = 2.0f; 
    [SerializeField] private float minVelocityToReact = 2.0f; 
    [SerializeField] private float hitSfxDuration = 1.5f;

    [Header("UI Elements")]
    public GameObject promptUI;        
    public GameObject defaultCursorObj;   
    public GameObject interactCursorObj;

    private float lastHitTime;

    void Start()
    {
        // Ensure UI is clean on startup
        if (promptUI) promptUI.SetActive(false);
        ResetCursors();
    }

    // --- IINTERACTABLE IMPLEMENTATION ---
    public void OnFocus()
    {
        // Only show prompt and change cursor if the game hasn't been started
        if (!Nets.IsMinigameActive)
        {
            if (promptUI) promptUI.SetActive(true);
            if (defaultCursorObj) defaultCursorObj.SetActive(false);
            if (interactCursorObj) interactCursorObj.SetActive(true);
        }
    }

    public void OnLoseFocus()
    {
        if (promptUI) promptUI.SetActive(false);
        if (!Nets.IsMinigameActive) ResetCursors();
    }

    public void OnInteract()
    {
        if (!Nets.IsMinigameActive)
        {
            // Hide the prompt and reset the cursor immediately upon starting
            if (promptUI) promptUI.SetActive(false);
            ResetCursors();

            // Kick off the minigame logic
            Nets firstHoop = FindFirstObjectByType<Nets>();
            if (firstHoop != null)
            {
                firstHoop.StartBasketballGame();
                TriggerGreeting(); 
            }
        }
    }

    private void ResetCursors() 
    {
        if (defaultCursorObj) defaultCursorObj.SetActive(true);
        if (interactCursorObj) interactCursorObj.SetActive(false);
    }

    // --- ANTI-SPAM COLLISION ---
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(garbageTag)) return;
        
        // Won't trigger if garbage is just lying there or barely rolling
        if (collision.relativeVelocity.magnitude < minVelocityToReact) return;

        if (Time.time < lastHitTime + hitCooldown) return;
        if (!NPCVoiceManager.instance.CanPlay()) return;

        lastHitTime = Time.time;
        RuntimeManager.PlayOneShot(FMODEvents.instance.GymTeacherHit, transform.position);
        NPCVoiceManager.instance.PlayForDuration(hitSfxDuration);
    }

    public void TriggerGreeting()
    {
        if (!NPCVoiceManager.instance.CanPlay()) return;

        RuntimeManager.PlayOneShot(FMODEvents.instance.GymTeacherGreet, transform.position);
        NPCVoiceManager.instance.PlayForDuration(5.5f);
    }
}