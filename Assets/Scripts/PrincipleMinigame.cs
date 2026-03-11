using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PrincipalMinigame : MonoBehaviour, IInteractable
{
    [Header("UI Elements")]
    public GameObject minigameHUD;   
    public GameObject promptUI;      
    public Image patienceFill;       

    [Header("Success UI")]
    public GameObject successUIPanel; 
    public TextMeshProUGUI remainingText; 

    [Header("Dual Cursor System")]
    public GameObject defaultCursorObj;   
    public GameObject interactCursorObj;  

    [Header("Settings")]
    public float maxPatience = 5000f;     
    public float hitPenalty = 25f;        
    public float messNearPenalty = 15f;   
    public float detectionRadius = 8f; 
    public int maxAllowedMesses = 6; // Total limit for dynamic spawns
    private bool hasWon = false;

    [Header("Pre-placed Initial Messes")]
    [Tooltip("Drag the Banana Mess objects already in your scene here (should be disabled by default)")]
    public List<GameObject> startingMesses = new List<GameObject>();

    [Header("Animations")]
    public Animator principalAnim;
    public string hitStateName = "Hit";   

    private float currentPatience;
    private bool isGameActive;
    private List<GameObject> activeMesses = new List<GameObject>(); 
    private Vector3 originalHUDPos; 

    void Start()
    {
        currentPatience = maxPatience;
        if (minigameHUD != null) {
            originalHUDPos = minigameHUD.GetComponent<RectTransform>().anchoredPosition;
            minigameHUD.SetActive(false);
        }
        if (successUIPanel) successUIPanel.SetActive(false);
        if (promptUI) promptUI.SetActive(false);
        
        // Ensure starting messes are hidden at the very beginning
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
        // Only reset cursors if we aren't in the game; 
        // otherwise, Banana.cs handles it.
        if (!isGameActive) ResetCursors(); 
    }

    public void OnInteract() 
    { 
        if (!isGameActive && !hasWon) StartMiniGame(); 
    }

    public void StartMiniGame() 
    {
        isGameActive = true;
        currentPatience = maxPatience;
        activeMesses.Clear();

        if (promptUI) promptUI.SetActive(false);
        if (minigameHUD) minigameHUD.SetActive(true);
    
        // 1. Enable and register the hand-placed messes
        ActivateInitialMesses();

        UpdateSuccessUI();
        ResetCursors();
    }

    void Update() {
        if (!isGameActive) return;

        // Cleanup any cleaned/destroyed bananas from the list
        activeMesses.RemoveAll(item => item == null);

        // Patience drain based on total active messes
        if (activeMesses.Count > 0) {
            currentPatience -= Time.deltaTime * messNearPenalty * activeMesses.Count;
        }

        UpdateUIFeedback();
        UpdateSuccessUI(); 

        if (currentPatience <= 0) EndGame();
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
        if (successUIPanel == null || remainingText == null) return;
    
        successUIPanel.SetActive(isGameActive);
        remainingText.text = activeMesses.Count.ToString();

        // WIN CONDITION: If game is active and we hit 0 messes
        if (isGameActive && activeMesses.Count == 0)
        {
            WinGame();
        }
    }

    void UpdateUIFeedback() {
        if (patienceFill == null) return;
        float ratio = currentPatience / maxPatience;
        patienceFill.fillAmount = ratio;

        RectTransform hudRect = minigameHUD.GetComponent<RectTransform>();
        if (ratio < 0.35f) {
            float intensity = Mathf.Lerp(12f, 2f, ratio / 0.35f); 
            hudRect.anchoredPosition = originalHUDPos + new Vector3(Random.Range(-intensity, intensity), Random.Range(-intensity, intensity), 0);
        } else {
            hudRect.anchoredPosition = originalHUDPos;
        }
    }

    public void GetHit() {
        if (principalAnim != null) principalAnim.Play(hitStateName, 0, 0f); 
        if (!isGameActive) return;
        currentPatience -= hitPenalty;
    }

    public bool IsGameActive() => isGameActive;
    
    public bool CanSpawnMessAt(Vector3 position) {
        if (!isGameActive) return false;
        if (activeMesses.Count >= maxAllowedMesses) return false;
    
        // 1. Check distance to Principal
        float distToPrincipal = Vector3.Distance(transform.position, position);
        if (distToPrincipal > detectionRadius) return false;

        // 2. Prevent stacking: Check distance to existing messes
        foreach (GameObject mess in activeMesses)
        {
            if (mess != null)
            {
                if (Vector3.Distance(mess.transform.position, position) < 1.2f) 
                {
                    return false; 
                }
            }
        }
        return true;
    }

    private void WinGame()
    {
        hasWon = true; 
        Debug.Log("Cleaned all messes! Students are stopping.");
    
        // Stop all students
        PooledThrower[] students = FindObjectsOfType<PooledThrower>();
        foreach (PooledThrower s in students)
        {
            s.StopThrowingPermanently();
        }

        EndGame(); 
    }
    
    private void StopAllStudents()
    {
        // Find every student thrower in the scene
        PooledThrower[] students = FindObjectsOfType<PooledThrower>();
    
        foreach (PooledThrower s in students)
        {
            // Cancel the next scheduled throw
            s.CancelInvoke("StartThrowCycle");
        
            // Optional: Force them into an idle state immediately
            Animator a = s.GetComponent<Animator>();
            if (a != null) a.Play("Idle"); // Use your actual Idle state name
        }
    }

    void EndGame() {
        isGameActive = false;
    
        // Force-close any open banana minigame
        Banana activeBanana = FindObjectOfType<Banana>();
        if (activeBanana != null)
        {
            activeBanana.SendMessage("EndMinigame", false, SendMessageOptions.DontRequireReceiver);
        }

        if (minigameHUD) minigameHUD.SetActive(false);
        if (successUIPanel) successUIPanel.SetActive(false);

        // Final cleanup: Destroy student clones and deactivate starting messes
        foreach (GameObject m in activeMesses)
        {
            if (m != null) 
            {
                if(startingMesses.Contains(m)) m.SetActive(false);
                else Destroy(m);
            }
        }
        activeMesses.Clear();
    
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