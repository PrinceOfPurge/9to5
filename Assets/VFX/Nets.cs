using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Nets : MonoBehaviour
{
    public static int TotalBucketsScored = 0;
    public static bool IsMinigameActive = false;
    public static bool IsMinigameWon = false;

    private static int currentBucketsNeeded = 0; 

    [Header("Settings")]
    public int initialBucketsNeeded = 3; 
    public int bucketsIncreasePerLevel = 2; 
    public int points = 150;
    public float buzzerGracePeriod = 2.0f; // NEW: Time allowed for mid-air shots to land

    [Header("Timer Settings")]
    public float timeLimit = 30f; 
    private static float gameEndTime; 
    private static bool isWaitingForBuzzer = false; // NEW: Track grace period

    [Header("UI Reference")]
    public GameObject uiCanvas; 
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public Color activeColor = Color.white;
    public Color timerActiveColor = new Color(1.0f, 0.55f, 0.0f); 
    public Color winColor = Color.green;
    public Color failColor = Color.red; 

    private ParticleSystem hoopParticles;
    private static List<Nets> allHoops = new List<Nets>();
    private List<GameObject> recentlyScoredGarbage = new List<GameObject>();
    private Coroutine hideUICoroutine;

    void Awake()
    {
        if (!allHoops.Contains(this)) allHoops.Add(this);
        if (currentBucketsNeeded == 0) currentBucketsNeeded = initialBucketsNeeded;
    }

    void Start()
    {
        hoopParticles = GetComponentInChildren<ParticleSystem>(true);
        if (uiCanvas != null) uiCanvas.SetActive(false);
        UpdateScoreUI();
    }

    void Update()
    {
        if (IsMinigameActive && !IsMinigameWon)
        {
            float timeLeft = gameEndTime - Time.time;

            if (timeLeft <= 0)
            {
                if (!isWaitingForBuzzer)
                {
                    // Start the grace period for mid-air shots
                    isWaitingForBuzzer = true;
                    StartCoroutine(BuzzerBeaterCountdown());
                }
                timeLeft = 0;
            }

            UpdateTimerUI(timeLeft);
        }
    }

    private IEnumerator BuzzerBeaterCountdown()
    {
        // Wait to see if a mid-air shot goes in
        yield return new WaitForSeconds(buzzerGracePeriod);
        
        // If they haven't won by the end of the grace period, they fail
        if (IsMinigameActive && !IsMinigameWon)
        {
            FailGame();
        }
        isWaitingForBuzzer = false;
    }

    public void StartBasketballGame()
    {
        // BLOCK START IF ALREADY WON IN THIS SCENE
        if (IsMinigameWon) return;

        TotalBucketsScored = 0;
        IsMinigameActive = true;
        IsMinigameWon = false;
        isWaitingForBuzzer = false;
        
        gameEndTime = Time.time + timeLimit;
        
        foreach (Nets hoop in allHoops)
        {
            if (hoop.hideUICoroutine != null) hoop.StopCoroutine(hoop.hideUICoroutine);
            if (hoop.uiCanvas != null) hoop.uiCanvas.SetActive(true);
            hoop.recentlyScoredGarbage.Clear(); 
            if (hoop.scoreText != null) hoop.scoreText.color = hoop.activeColor;
            if (hoop.timerText != null) hoop.timerText.color = hoop.timerActiveColor;
            hoop.UpdateScoreUI();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Garbage"))
        {
            if (IsMinigameWon) return;
            if (recentlyScoredGarbage.Contains(other.gameObject)) return;

            // Allow scoring during active game OR during the buzzer grace period
            if (IsMinigameActive || isWaitingForBuzzer)
            {
                recentlyScoredGarbage.Add(other.gameObject);
                StartCoroutine(ClearGarbageMemory(other.gameObject));

                TotalBucketsScored++;
                foreach (Nets hoop in allHoops) hoop.UpdateScoreUI();
                
                if (hoopParticles != null) hoopParticles.Play();
                if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Swish, transform.position);

                if (TotalBucketsScored >= currentBucketsNeeded)
                {
                    CompleteGame();
                }
            }
        }
    }

    void CompleteGame()
    {
        StopAllCoroutines(); // Stop the BuzzerBeater failure countdown
        IsMinigameWon = true;
        IsMinigameActive = false;
        isWaitingForBuzzer = false;

        currentBucketsNeeded += bucketsIncreasePerLevel;

        if (SinglePlayerModeManager.Instance != null) SinglePlayerModeManager.Instance.SinglePlayerScore += points;

        foreach (Nets hoop in allHoops)
        {
            hoop.UpdateScoreUI();
            if (hoop.timerText != null) hoop.timerText.color = hoop.winColor;
            hoop.hideUICoroutine = hoop.StartCoroutine(hoop.HideUIAfterDelay(3f));
        }
        
        PASystem pa = FindFirstObjectByType<PASystem>();
        if(pa != null) pa.CheckForInstantUpdate();
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            int displayGoal = IsMinigameWon ? TotalBucketsScored : currentBucketsNeeded;
            scoreText.text = $"{TotalBucketsScored} / {displayGoal}";
            scoreText.color = IsMinigameWon ? winColor : activeColor;
        }
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText != null) timerText.text = isWaitingForBuzzer ? "0.0!" : timeRemaining.ToString("F1");
    }

    void FailGame()
    {
        IsMinigameActive = false;
        isWaitingForBuzzer = false;
        foreach (Nets hoop in allHoops)
        {
            if (hoop.scoreText != null) hoop.scoreText.color = hoop.failColor;
            if (hoop.timerText != null) { hoop.timerText.color = hoop.failColor; hoop.timerText.text = "0.0"; }
            hoop.hideUICoroutine = hoop.StartCoroutine(hoop.HideUIAfterDelay(3f));
        }
    }

    private void TriggerTeacherGreeting()
    {
        GymTeacherVO teacher = FindFirstObjectByType<GymTeacherVO>();
        if (teacher != null) teacher.TriggerGreeting();
    }

    private IEnumerator ClearGarbageMemory(GameObject garbageObj) { yield return new WaitForSeconds(2.0f); if (recentlyScoredGarbage.Contains(garbageObj)) recentlyScoredGarbage.Remove(garbageObj); }
    private IEnumerator HideUIAfterDelay(float delay) { yield return new WaitForSeconds(delay); if (uiCanvas != null) uiCanvas.SetActive(false); }
    private void OnDestroy() { if (allHoops.Contains(this)) allHoops.Remove(this); }
}