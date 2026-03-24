using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FMOD.Studio;
using Random = UnityEngine.Random;

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
    public bool hasWon { get; private set; } = false;
    public static PrincipalMinigame instance;

    [Header("Spawn System")]
    public GameObject messPrefab; 
    private Transform[] spawnPoints;

    [Header("Animations")]
    public Animator principalAnim;
    public string hitStateName = "Hit";   

    private float currentPatience;
    private bool isGameActive;
    private List<GameObject> activeMesses = new List<GameObject>(); 
    private Camera mainCam;

    // Audio shtuff
    private float idleTimer;
    private float nextIdleTime = 10f;
    private EventInstance voiceInstance;
    private bool hasGreeted = false; // Track the initial greeting
    private Transform playerTransform;

    void Start()
    {
        mainCam = Camera.main;
        currentPatience = maxPatience;
        
        // Find player for distance-based greeting
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) playerTransform = playerObj.transform;

        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("MessSpawn");
        spawnPoints = new Transform[spawnObjects.Length];
        for (int i = 0; i < spawnObjects.Length; i++)
        {
            spawnPoints[i] = spawnObjects[i].transform;
        }
        if (worldMessCountUI) worldMessCountUI.SetActive(false);
        if (worldTimerBar) worldTimerBar.SetActive(false);
        if (screenMessCountUI) screenMessCountUI.SetActive(false);
        if (screenTimerBar) screenTimerBar.SetActive(false);
        if (promptUI) promptUI.SetActive(false);
        
        if (worldMessCountUI) worldMessCountUI.SetActive(false);
        
        ResetCursors();
    }

    private void Awake()
    {
        instance = this;
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
        hasWon = false; 
        currentPatience = maxPatience;
        activeMesses.Clear();

        if (promptUI) promptUI.SetActive(false);
        if (externalGarbageUI) externalGarbageUI.SetActive(false); 
        
        PlayVoice(1, true); // Force start line

        ActivateInitialMesses();
        UpdateSuccessUI();
        ResetCursors();
    }

    void Update() {
        
        if (!isGameActive && !hasWon && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            // 100% Greet when player first enters detection range
            if (!hasGreeted && dist <= detectionRadius)
            {
                PlayVoice(0, true);
                hasGreeted = true;
                idleTimer = 0f;
            }

            // Random idles only if already greeted and still in range
            idleTimer += Time.deltaTime;
            if (idleTimer >= nextIdleTime && dist <= detectionRadius)
            {
                PlayVoice(0, false); // Don't interrupt if already talking
                idleTimer = 0;
                nextIdleTime = Random.Range(15f, 25f);
            }
        }

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
        if (spawnPoints == null || messPrefab == null) return;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                GameObject newMess = Instantiate(messPrefab, point.position, point.rotation);
                RegisterMess(newMess);
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
        
        // Hits always interrupt whatever else he's saying
        PlayVoice(2, true);

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
        
        PlayVoice(3, true); // Force victory line

        PooledThrower[] students = FindObjectsOfType<PooledThrower>();
        foreach (PooledThrower s in students) s.StopThrowingPermanently();
        EndGame(); 
    }

    void EndGame() {
        isGameActive = false;
    
        Banana[] activeBananas = FindObjectsOfType<Banana>();
        foreach(Banana b in activeBananas)
        {
            b.SendMessage("EndMinigame", false, SendMessageOptions.DontRequireReceiver);
        }

        if (worldMessCountUI) worldMessCountUI.SetActive(false);
        if (worldTimerBar) worldTimerBar.SetActive(false);
        if (screenMessCountUI) screenMessCountUI.SetActive(false);
        if (screenTimerBar) screenTimerBar.SetActive(false);
        
        if (externalGarbageUI) externalGarbageUI.SetActive(true);

        foreach (GameObject m in activeMesses)
        {
            if (m != null) Destroy(m);
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
    
    private void PlayVoice(int state, bool forceInterrupt = false)
    {
        if (voiceInstance.isValid())
        {
            PLAYBACK_STATE pbState;
            voiceInstance.getPlaybackState(out pbState);

            // If already playing and we aren't forcing an interrupt (like a Hit), just exit
            if (pbState == PLAYBACK_STATE.PLAYING && !forceInterrupt) return;

            voiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            voiceInstance.release();
        }

        voiceInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.Principal);
        voiceInstance.setParameterByName("PrincipalState", (float)state);
        voiceInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
    
        voiceInstance.start();
        voiceInstance.release(); 
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}