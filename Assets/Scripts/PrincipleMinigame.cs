using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PrincipalMinigame : MonoBehaviour, IInteractable
{
    [Header("UI Elements")]
    public GameObject promptUI;        
    public GameObject externalGarbageUI; 

    [Header("World Space HUD")]
    public GameObject worldMessCountUI; 
    public GameObject worldTimerBar;    
    public Image patienceFill;          
    public TextMeshProUGUI remainingText; 

    [Header("Screen Space HUD")]
    public GameObject screenMessCountUI; 
    public GameObject screenTimerBar;    
    public Image screenPatienceFill;     
    public TextMeshProUGUI screenRemainingText;

    [Header("Dual Cursor System")]
    public GameObject defaultCursorObj;   
    public GameObject interactCursorObj;  

    [Header("Settings")]
    public float maxPatience = 5000f;     
    public float hitPenalty = 25f;        
    public float messNearPenalty = 15f;   
    public float detectionRadius = 8f; 
    public int maxAllowedMesses = 6; 
    private bool hasWon = false;

    [Header("Pre-placed Initial Messes")]
    public List<GameObject> startingMesses = new List<GameObject>();

    [Header("Animations")]
    public Animator principalAnim;
    public string hitStateName = "Hit";   

    private float currentPatience;
    private bool isGameActive;
    private List<GameObject> activeMesses = new List<GameObject>(); 
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        currentPatience = maxPatience;

        if (worldMessCountUI) worldMessCountUI.SetActive(false);
        if (worldTimerBar) worldTimerBar.SetActive(false);
        if (screenMessCountUI) screenMessCountUI.SetActive(false);
        if (screenTimerBar) screenTimerBar.SetActive(false);
        if (promptUI) promptUI.SetActive(false);
        
        foreach(GameObject m in startingMesses) if(m != null) m.SetActive(false);
        
        ResetCursors();
    }

    public void OnFocus() 
    {
        if (hasWon || isGameActive) return;
        if (promptUI) promptUI.SetActive(true);
        if (defaultCursorObj) defaultCursorObj.SetActive(false);
        if (interactCursorObj) interactCursorObj.SetActive(true);
    }

    public void OnLoseFocus() { 
        if (promptUI) promptUI.SetActive(false); 
        if (!isGameActive) ResetCursors(); 
    }

    public void OnInteract() 
    { 
        if (!isGameActive && !hasWon) StartMiniGame(); 
    }

    public void StartMiniGame() 
    {
        isGameActive = true;
        hasWon = false; // Reset win state so we can play again
        currentPatience = maxPatience;
        activeMesses.Clear();

        if (promptUI) promptUI.SetActive(false);
        if (externalGarbageUI) externalGarbageUI.SetActive(false); 
    
        ActivateInitialMesses();
        UpdateSuccessUI();
        ResetCursors();
    }

    void Update() {
        if (!isGameActive) return;

        activeMesses.RemoveAll(item => item == null);

        if (activeMesses.Count > 0) {
            currentPatience -= Time.deltaTime * messNearPenalty * activeMesses.Count;
        }

        HandleHUDVisibility();
        UpdateUIFeedback();
        UpdateSuccessUI(); 

        if (currentPatience <= 0) EndGame();
    }

    private void HandleHUDVisibility()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 screenPoint = mainCam.WorldToViewportPoint(transform.position + Vector3.up * 1.5f);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if (worldMessCountUI) worldMessCountUI.SetActive(onScreen);
        if (worldTimerBar) worldTimerBar.SetActive(onScreen);
        if (screenMessCountUI) screenMessCountUI.SetActive(!onScreen);
        if (screenTimerBar) screenTimerBar.SetActive(!onScreen);
    }

    private void ActivateInitialMesses()
    {
        foreach (GameObject mess in startingMesses)
        {
            if (mess != null)
            {
                mess.SetActive(true);
                RegisterMess(mess);
            }
        }
    }

    public void RegisterMess(GameObject mess) {
        if (!activeMesses.Contains(mess)) {
            activeMesses.Add(mess);
        }
    }

    public void NotifyMessCleaned() {
        UpdateSuccessUI();
    }

    public void UpdateSuccessUI() 
    {
        string count = activeMesses.Count.ToString();
        if (remainingText) remainingText.text = count;
        if (screenRemainingText) screenRemainingText.text = count;

        if (isGameActive && activeMesses.Count == 0)
        {
            WinGame();
        }
    }

    void UpdateUIFeedback() {
        float ratio = 1f - (currentPatience / maxPatience);
        if (patienceFill) patienceFill.fillAmount = ratio;
        if (screenPatienceFill) screenPatienceFill.fillAmount = ratio;
    }

    public void GetHit() {
        if (principalAnim != null) principalAnim.Play(hitStateName, 0, 0f); 
        if (!isGameActive) return;
        currentPatience -= hitPenalty;
    }

    public bool IsGameActive() => isGameActive;
    
    public bool CanSpawnMessAt(Vector3 position) {
        if (!isGameActive || activeMesses.Count >= maxAllowedMesses) return false;
        if (Vector3.Distance(transform.position, position) > detectionRadius) return false;

        foreach (GameObject mess in activeMesses)
        {
            if (mess != null && Vector3.Distance(mess.transform.position, position) < 1.2f) return false; 
        }
        return true;
    }

    private void WinGame()
    {
        hasWon = true; 
        PooledThrower[] students = FindObjectsOfType<PooledThrower>();
        foreach (PooledThrower s in students) s.StopThrowingPermanently();
        EndGame(); 
    }

    void EndGame() {
        isGameActive = false;
    
        // 1. FORCE SHUTDOWN of the cleaning minigame
        // We look for any object with the Banana script and call its end method
        Banana[] activeBananas = FindObjectsOfType<Banana>();
        foreach(Banana b in activeBananas)
        {
            // Assuming your Banana script has a method like 'FailMinigame' or 'EndMinigame'
            // Using SendMessage as a backup, but direct calling is better if possible
            b.SendMessage("EndMinigame", false, SendMessageOptions.DontRequireReceiver);
        }

        // 2. Hide all Minigame UI
        if (worldMessCountUI) worldMessCountUI.SetActive(false);
        if (worldTimerBar) worldTimerBar.SetActive(false);
        if (screenMessCountUI) screenMessCountUI.SetActive(false);
        if (screenTimerBar) screenTimerBar.SetActive(false);
        
        // 3. Restore Gameplay UI
        if (externalGarbageUI) externalGarbageUI.SetActive(true);

        // 4. Cleanup Messes
        foreach (GameObject m in activeMesses)
        {
            if (m != null) 
            {
                if(startingMesses.Contains(m)) m.SetActive(false);
                else Destroy(m);
            }
        }
        activeMesses.Clear();

        // 5. Restore Player Controls
        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.enabled = true;

        ResetCursors();
    }

    private void ResetCursors() {
        if (defaultCursorObj) defaultCursorObj.SetActive(true);
        if (interactCursorObj) interactCursorObj.SetActive(false);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}