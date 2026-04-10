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
        if (promptUI) promptUI.SetActive(false);
        ResetCursors();
    }

    public void OnFocus()
    {
        if (!Nets.IsMinigameActive && !Nets.IsMinigameWon)
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
        if (!Nets.IsMinigameActive && !Nets.IsMinigameWon)
        {
            if (promptUI) promptUI.SetActive(false);
            ResetCursors();

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(garbageTag)) return;
        
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