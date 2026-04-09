using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerModeManager : MonoBehaviour
{
    [Header("UI & Scoring")]
    public GameObject BagsRemainingUIContainer; 
    public TextMeshProUGUI BagsRemainingText;
    
    public int BagsRemaining;
    public int SinglePlayerScore;
    public int ActiveStudents = 0; // If you aren't strictly counting them down to 0, we'll ignore this for the win

    public int level = 1;
    public int PlayerMoney;

    private bool gameEnded = false;
    private float sceneLoadTime; 

    public static SinglePlayerModeManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BagsRemainingUIContainer = GameObject.Find("UI_BagsRemaining"); 
        BagsRemainingText = GameObject.Find("BagsRemainingText")?.GetComponent<TextMeshProUGUI>();

        // Reset state for new level
        if (scene.name == "SinglePlayerMode")
        {
            gameEnded = false;
            BagsRemaining = 0; 
            SinglePlayerScore = 0;
            sceneLoadTime = Time.time; 

            // Reset minigame statics on level load
            Nets.IsMinigameActive = false;
            Nets.IsMinigameWon = false;
            Nets.TotalBucketsScored = 0;
        }
    }

    void Update()
    {
        if (gameEnded) return;

        UpdateBagsRemainingUI();
        EndTheGame();
    }
    
    void UpdateBagsRemainingUI()
    {
        GameObject uiToToggle = BagsRemainingUIContainer != null ? BagsRemainingUIContainer : (BagsRemainingText != null ? BagsRemainingText.gameObject : null);

        if (uiToToggle != null && BagsRemainingText != null)
        {
            if (BagsRemaining <= 0) uiToToggle.SetActive(false); 
            else
            {
                uiToToggle.SetActive(true);
                BagsRemainingText.text = "X" + BagsRemaining;
            }
        }
    }

    void EndTheGame()
    {
        if (Time.time - sceneLoadTime < 3.0f) return; 

        if (Input.GetKeyDown(KeyCode.B))
        {
            DisplayResults();
            return;
        }

        // PHYSICAL TASKS CHECK
        // We removed ActiveStudents from this check because they wander forever
        bool tasksPhysicallyDone = (BagsRemaining <= 0);

        if (tasksPhysicallyDone && !gameEnded)
        {
            if (PASystem.Instance != null)
            {
                if (PASystem.Instance.finalAnnouncementFinished)
                {
                    DisplayResults();
                }
            }
            else
            {
                DisplayResults();
            }
        }
    }

    void DisplayResults()
    {
        if (gameEnded) return; 

        Debug.Log("LEVEL COMPLETE: Transitioning to Shop...");
        gameEnded = true;
        level++;

        PlayerMoney += SinglePlayerScore;
        StartCoroutine(GotoShop());
    }

    IEnumerator GotoShop()
    {
        BagsRemaining = 0; 
        yield return new WaitForSeconds(1.5f);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // CRITICAL: Ensure this string matches your Scene name in Build Settings exactly!
        SceneManager.LoadScene("SinglePlayerUpgradeShop");
    }
}