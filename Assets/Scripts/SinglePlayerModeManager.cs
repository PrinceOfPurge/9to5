using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerModeManager : MonoBehaviour
{
    [SerializeField]
    // Timer
    public TextMeshProUGUI TimerText;
    public float TimerValue;
    public bool TimerisRunning;

    // Score
    public TextMeshProUGUI BagsRemainingText;
    public int BagsRemaining;
    public int SinglePlayerScore;


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
        // Try to find TimerText again when a scene loads
        TimerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        BagsRemainingText = GameObject.Find("BagsRemainingText")?.GetComponent<TextMeshProUGUI>();

        // If we're back in the gameplay scene, restart the timer
        if (scene.name == "SinglePlayerMode")
        {
            TimerValue = 120f;
            TimerisRunning = true;
            BagsRemaining = 6;
        }
    }



    void Start()
    {
        TimerisRunning = true;
        TimerValue = 120;
    }

    void Update()
    {
        

        if (TimerisRunning)
        {
            TimerValue -= Time.deltaTime;
            TimerValue = Mathf.Max(TimerValue, 0f); // prevent negative
        }
        
        if (TimerisRunning == false) {
            TimerValue = 120;
        }

        UpdateTimerUI();
        UpdateBagsRemainingUI();

        EndTheGame();
        
    }
    
    //---------------------------------------------------------

    void UpdateTimerUI()
    {
        //TimerText.text = "TIMER: " + TimerValue.ToString();
        SinglePlayerModeManager.Instance.TimerText.text = FormatTime(SinglePlayerModeManager.Instance.TimerValue);

    } 
    
    // MM:SS formatter
    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0}:{1:00}", minutes, seconds);
    }

    void UpdateBagsRemainingUI()
    {
        SinglePlayerModeManager.Instance.BagsRemainingText.text = "X" + SinglePlayerModeManager.Instance.BagsRemaining;
    }

    void EndTheGame()
    {
        // When 0 bags remaining, or Time Over, End the Game
        //if (BagsRemaining == 0)
        if (Input.GetKeyDown(KeyCode.B) || SinglePlayerModeManager.Instance.BagsRemaining == 0)
        {
            TimerisRunning = false;

            DisplayResults();
        }
        if (SinglePlayerModeManager.Instance.BagsRemaining == 0)
        {
            TimerisRunning = false;

            DisplayResults();
        }




        //Show results UI

        //Go to shop


    }
    
    

    void DisplayResults()
    {
        // Show Results UI
        Debug.Log("END DA GAME");

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
