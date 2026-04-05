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
    public int points = 150;

    [Header("Timer Settings")]
    public float timeLimit = 30f; 
    private static float gameEndTime; 

    [Header("UI Reference (World Space)")]
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

    void Update()
    {
        if (IsMinigameActive && !IsMinigameWon)
        {
            float timeLeft = gameEndTime - Time.time;

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                FailGame();
            }

            UpdateTimerUI(timeLeft);
        }
    }

    public void StartBasketballGame()
    {
        if (IsMinigameWon) return;
        
        TotalBucketsScored = 0;
        IsMinigameActive = true;
        
        gameEndTime = Time.time + timeLimit;
        
        foreach (Nets hoop in allHoops)
        {
            if (hoop.uiCanvas != null) hoop.uiCanvas.SetActive(true);
            hoop.recentlyScoredGarbage.Clear(); 
            
            if (hoop.scoreText != null) hoop.scoreText.color = hoop.activeColor;
            
            if (hoop.timerText != null) hoop.timerText.color = hoop.timerActiveColor;

            hoop.UpdateScoreUI();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsMinigameWon || !IsMinigameActive) return;

        if (other.CompareTag("Garbage"))
        {
            if (recentlyScoredGarbage.Contains(other.gameObject)) return;

            recentlyScoredGarbage.Add(other.gameObject);
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

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText != null)
        {
            timerText.text = timeRemaining.ToString("F1");
        }
    }

    void FailGame()
    {
        IsMinigameActive = false;

        foreach (Nets hoop in allHoops)
        {
            if (hoop.scoreText != null) hoop.scoreText.color = hoop.failColor;
            if (hoop.timerText != null) 
            {
                hoop.timerText.color = hoop.failColor;
                hoop.timerText.text = "0.0";
            }

            hoop.StartCoroutine(hoop.HideUIAfterDelay(3f));
        }
    }

    void CompleteGame()
    {
        IsMinigameWon = true;
        IsMinigameActive = false;

        if (SinglePlayerModeManager.Instance != null)
        {
            SinglePlayerModeManager.Instance.SinglePlayerScore += points;
        }

        foreach (Nets hoop in allHoops)
        {
            hoop.UpdateScoreUI();
            if (hoop.timerText != null) hoop.timerText.color = hoop.winColor;
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