using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerModeManager : MonoBehaviour
{
    // Score & Bags
    public TextMeshProUGUI BagsRemainingText;
    public int BagsRemaining;
    public int SinglePlayerScore;
    public int ActiveStudents = 0;

    public int level = 1;
    public int PlayerMoney;

    private bool gameEnded = false;

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
        BagsRemainingText = GameObject.Find("BagsRemainingText")?.GetComponent<TextMeshProUGUI>();

        // If we're back in the gameplay scene, reset the level variables
        if (scene.name == "SinglePlayerMode")
        {
            gameEnded = false;
            BagsRemaining = 1;
            SinglePlayerScore = 0;
        }
    }

    void Start()
    {
        // Start logic (if needed in the future) goes here
    }

    void Update()
    {
        if (gameEnded)
            return;

        UpdateBagsRemainingUI();
        EndTheGame();
    }
    
    //---------------------------------------------------------

    void UpdateBagsRemainingUI()
    {
        // Directly update the text if the reference exists
        if (BagsRemainingText != null)
        {
            BagsRemainingText.text = "X" + BagsRemaining;
        }
    }

    void EndTheGame()
    {
        // The game only ends if there are 0 bags AND 0 students still making messes
        // (Or if the player uses the 'B' debug key)
        if (Input.GetKeyDown(KeyCode.B) || (BagsRemaining <= 0 && ActiveStudents <= 0))
        {
            DisplayResults();
        }
    }

    void DisplayResults()
    {
        // Prevent this from running multiple times
        if (gameEnded) return; 

        Debug.Log("END DA GAME");

        gameEnded = true;
        level++;

        // Add score to money (Timer bonus has been removed)
        PlayerMoney += SinglePlayerScore;

        StartCoroutine(GotoShop());
    }

    IEnumerator GotoShop()
    {
        BagsRemaining = 1;

        yield return new WaitForSeconds(3f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("SinglePlayerUpgradeShop");
    }
}