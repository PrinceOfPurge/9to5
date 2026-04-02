using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerModeManager : MonoBehaviour
{
    // Score & Bags
    public GameObject BagsRemainingUIContainer; 
    public TextMeshProUGUI BagsRemainingText;
    
    public int BagsRemaining;
    public int SinglePlayerScore;
    public int ActiveStudents = 0;

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

        if (scene.name == "SinglePlayerMode")
        {
            gameEnded = false;
            BagsRemaining = 0; 
            SinglePlayerScore = 0;
            sceneLoadTime = Time.time; 
        }
    }

    void Update()
    {
        if (gameEnded)
            return;

        UpdateBagsRemainingUI();
        EndTheGame();
    }
    
    void UpdateBagsRemainingUI()
    {
        GameObject uiToToggle = BagsRemainingUIContainer != null ? BagsRemainingUIContainer : (BagsRemainingText != null ? BagsRemainingText.gameObject : null);

        if (uiToToggle != null && BagsRemainingText != null)
        {
            if (BagsRemaining <= 0)
            {
                uiToToggle.SetActive(false); 
            }
            else
            {
                uiToToggle.SetActive(true);
                BagsRemainingText.text = "X" + BagsRemaining;
            }
        }
    }

    void EndTheGame()
    {
        // Don't check for end game immediately after scene loads
        if (Time.time - sceneLoadTime < 2.0f) return; 

        // Debug key
        if (Input.GetKeyDown(KeyCode.B))
        {
            DisplayResults();
            return;
        }

        // --- NEW LOGIC ---
        // 1. Check if the physical tasks are finished
        bool tasksPhysicallyDone = (BagsRemaining <= 0 && ActiveStudents <= 0);

        if (tasksPhysicallyDone && !gameEnded)
        {
            // 2. Check for the PA System. 
            // We only end if the PA System has finished its final victory broadcast.
            if (PASystem.Instance != null)
            {
                if (PASystem.Instance.finalAnnouncementFinished)
                {
                    DisplayResults();
                }
            }
            else
            {
                // Fallback: If there is no PA System in this scene, end the game normally.
                DisplayResults();
            }
        }
    }

    void DisplayResults()
    {
        if (gameEnded) return; 

        Debug.Log("END DA GAME - Tasks and PA Announcement Complete");

        gameEnded = true;
        level++;

        PlayerMoney += SinglePlayerScore;

        StartCoroutine(GotoShop());
    }

    IEnumerator GotoShop()
    {
        // Safety reset
        BagsRemaining = 0; 

        // Short delay before loading the shop (PA System already provided the main delay)
        yield return new WaitForSeconds(1.5f);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("SinglePlayerUpgradeShop");
    }
}