using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Nets : MonoBehaviour
{
    public static int TotalBucketsScored = 0;
    public static bool IsMinigameActive = false;
    public static bool IsMinigameWon = false;

    [Header("Settings")]
    public int bucketsNeeded = 3;

    [Header("UI Reference (World Space)")]
    public GameObject uiCanvas; 
    public TextMeshProUGUI scoreText;
    public Color activeColor = Color.white;
    public Color winColor = Color.green;

    private ParticleSystem hoopParticles;
    private static List<Nets> allHoops = new List<Nets>();
    
    // --- NEW: Double-Count Protection ---
    private List<GameObject> recentlyScoredGarbage = new List<GameObject>();

    void Awake()
    {
        if (!allHoops.Contains(this)) allHoops.Add(this);
    }

    void Start()
    {
        hoopParticles = GetComponentInChildren<ParticleSystem>(true);
        if (uiCanvas != null) uiCanvas.SetActive(false);
        UpdateScoreUI();
    }

    public void StartBasketballGame()
    {
        if (IsMinigameWon) return;
        
        TotalBucketsScored = 0;
        IsMinigameActive = true;
        
        foreach (Nets hoop in allHoops)
        {
            if (hoop.uiCanvas != null) hoop.uiCanvas.SetActive(true);
            hoop.UpdateScoreUI();
            hoop.recentlyScoredGarbage.Clear(); // Clear the list on start
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsMinigameWon || !IsMinigameActive) return;

        if (other.CompareTag("Garbage"))
        {
            // NEW: If we already counted this exact piece of trash, ignore it!
            if (recentlyScoredGarbage.Contains(other.gameObject)) return;

            // Add it to the list to prevent double-counting
            recentlyScoredGarbage.Add(other.gameObject);
            
            // Start a timer to "forget" this trash after 2 seconds 
            // (so if the player picks it up and throws it again, it still works)
            StartCoroutine(ClearGarbageMemory(other.gameObject));

            TotalBucketsScored++;
            
            foreach (Nets hoop in allHoops)
            {
                hoop.UpdateScoreUI();
            }
            
            if (hoopParticles != null) hoopParticles.Play();
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Swish, transform.position);

            if (TotalBucketsScored >= bucketsNeeded)
            {
                CompleteGame();
            }
        }
    }

    // --- NEW: Forgetting the garbage after a short delay ---
    private IEnumerator ClearGarbageMemory(GameObject garbageObj)
    {
        yield return new WaitForSeconds(2.0f);
        if (recentlyScoredGarbage.Contains(garbageObj))
        {
            recentlyScoredGarbage.Remove(garbageObj);
        }
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{TotalBucketsScored} / {bucketsNeeded}";
            scoreText.color = IsMinigameWon ? winColor : activeColor;
        }
    }

    void CompleteGame()
    {
        IsMinigameWon = true;
        IsMinigameActive = false;

        foreach (Nets hoop in allHoops)
        {
            hoop.UpdateScoreUI();
        }
        
        PASystem pa = FindFirstObjectByType<PASystem>();
        if(pa != null) pa.CheckForInstantUpdate();
    }

    private IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (uiCanvas != null) uiCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        if (allHoops.Contains(this)) allHoops.Remove(this);
    }
}