using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode
{
    Tutorial,
    SinglePlayer,
    LANMultiplayer
}

public enum GameState
{
    MainMenu,
    ModeSelect,
    Options,
    Loading,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public GameMode currentMode;
    public GameState currentState;

    [Header("References")]
    public MenuManager menuManager;
    public AudioManager audioManager;
    // public ScoreManager scoreManager;
    // public NetworkManager networkManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentState = GameState.MainMenu;
    }

    // Loads current Gamemode/Scene ------------------  

    public void LoadTitleScene()
    {
        currentState = GameState.MainMenu;
        Debug.Log("Title (Scene) Loaded");
        SceneManager.LoadScene("TitleScreen");
    }

    public void LoadModeSelectScene()
    {
        currentState = GameState.ModeSelect;
        Debug.Log("Mode Select (Scene) Loaded");
        SceneManager.LoadScene("ModeSelect");
    }
    public void LoadOptionsScene()
    {
        currentState = GameState.Options;
        Debug.Log("Options (Scene) Loaded");
        SceneManager.LoadScene("Options");
    }  
    
    public void LoadTutorial()
    {
        currentMode = GameMode.Tutorial;
        Debug.Log("Tutorial (Scene) Loaded");
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadSinglePlayer()
    {
        currentMode = GameMode.SinglePlayer;
        Debug.Log("SinglePlayerLevel1 (Scene) Loaded");
        SceneManager.LoadScene("SinglePlayerMode");
    }

    public void LoadSinglePlayerShop()
    {
        currentMode = GameMode.SinglePlayer;
        Debug.Log("SinglePlayerLevel1 (Scene) Loaded");
        SceneManager.LoadScene("SinglePlayerUpgradeShop");
    }

    public void LoadLANMultiplayer(bool isHost)
    {
        /*currentMode = GameMode.LANMultiplayer;

        if (isHost)
            networkManager.StartHost();
        else
            networkManager.StartClient();

        LoadScene("MultiplayerScene");*/
    }

    public void LoadTestMultiplayer()
    {
        currentMode = GameMode.SinglePlayer;
        Debug.Log("Multiplayer (Scene) Loaded");
        SceneManager.LoadScene("MultiplayerTest");
    }

    public void QuitGame()
    {
        Application.Quit();

    }

    // Tracks current Gamestate ------------------    

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0;
        //menuManager.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1;
        //menuManager.ShowPauseMenu(false);
    }

    public void EndGame()
    {
        currentState = GameState.GameOver;
        //uIManager.ShowGameOverScreen();
    }

    void LoadScene(string sceneName)
    {
        currentState = GameState.Loading;
        SceneManager.LoadScene(sceneName);
    }

}