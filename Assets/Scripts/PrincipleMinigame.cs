using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 
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
    public int points = 250;
    public float maxPatience = 5000f;     
    public float hitPenalty = 25f;        
    public float messNearPenalty = 15f;   
    public float detectionRadius = 8f; 
    public int maxAllowedMesses = 6; 
    public float patienceRestorePerClean = 800f; 
    public float bounceForce = 6f; // NEW: How hard the garbage bounces back
    
    [Header("Boundary Visuals & Constraints")]
    public LineRenderer boundaryLine;
    public int lineSegments = 50; 
    public float yOffset = 0.05f;
    public float warningDistance = 2.5f; 
    public Color normalLineColor = Color.white;
    public Color warningLineColor = Color.red;

    [Header("Audio Tweak Settings")]
    public float winGracePeriod = 1.5f; 

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
    
    public bool isMinigameActive => isGameActive;
    
    private List<GameObject> activeMesses = new List<GameObject>(); 
    private Camera mainCam;

    private float idleTimer;
    private float nextIdleTime = 10f;
    private EventInstance voiceInstance;
    private bool hasGreeted = false; 
    private Transform playerTransform;
    private bool isInvulnerableToAudio = false; 

    [Header("Mess VFX")]
    [Tooltip("ParticleSystem prefab to play when an individual mess is destroyed.")]
    public ParticleSystem messDestroyVfxPrefab;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;
        currentPatience = maxPatience;
        
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
        
        if (boundaryLine != null)
        {
            DrawBoundaryRing();
            boundaryLine.enabled = false;
            boundaryLine.startColor = normalLineColor;
            boundaryLine.endColor = normalLineColor;
        }
        
        ResetCursors();
    }

    private void DrawBoundaryRing()
    {
        if (boundaryLine == null) return;
        boundaryLine.positionCount = lineSegments + 1;
        boundaryLine.useWorldSpace = true;
        float angle = 0f;
        for (int i = 0; i < (lineSegments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * detectionRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * detectionRadius;
            Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + yOffset, transform.position.z + z);
            boundaryLine.SetPosition(i, pos);
            angle += (360f / lineSegments);
        }
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
        
        if (boundaryLine != null)
        {
            boundaryLine.enabled = true;
            boundaryLine.startColor = normalLineColor;
            boundaryLine.endColor = normalLineColor;
        }

        PlayVoice(1, true); 
        ActivateInitialMesses();
        UpdateSuccessUI();
        ResetCursors();
    }

    void Update() {
        if (!isGameActive && !hasWon && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (!hasGreeted && dist <= detectionRadius)
            {
                PlayVoice(0, true);
                hasGreeted = true;
                idleTimer = 0f;
            }
            idleTimer += Time.deltaTime;
            if (idleTimer >= nextIdleTime && dist <= detectionRadius)
            {
                PlayVoice(0, false); 
                idleTimer = 0;
                nextIdleTime = Random.Range(15f, 25f);
            }
        }

        if (!isGameActive) return;

        if (playerTransform != null)
        {
            // Only measure horizontal distance to avoid false positives from jumping
            Vector3 centerFlat = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 playerFlat = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
            float playerDist = Vector3.Distance(centerFlat, playerFlat);
            
            bool isNearEdge = playerDist > (detectionRadius - warningDistance);

            // Handle Line Color
            if (boundaryLine != null)
            {
                Color targetColor = isNearEdge ? warningLineColor : normalLineColor;
                boundaryLine.startColor = targetColor;
                boundaryLine.endColor = targetColor;
            }

            // Forcefully clamp player inside the circle
            if (playerDist > detectionRadius)
            {
                Vector3 directionFromCenter = playerFlat - centerFlat;
                directionFromCenter = directionFromCenter.normalized;

                Vector3 clampedPosition = transform.position + (directionFromCenter * detectionRadius);
                clampedPosition.y = playerTransform.position.y; // Keep player's current jump height

                playerTransform.position = clampedPosition;
            }
        }

        // Handle out-of-bounds messes
        for (int i = activeMesses.Count - 1; i >= 0; i--)
        {
            if (activeMesses[i] != null)
            {
                float distToMess = Vector3.Distance(transform.position, activeMesses[i].transform.position);
                if (distToMess > detectionRadius)
                {
                    Rigidbody rb = activeMesses[i].GetComponent<Rigidbody>();
                    
                    // Check if held by player (has a parent or Rigidbody is currently kinematic)
                    bool isHeld = (activeMesses[i].transform.parent != null) || (rb != null && rb.isKinematic);

                    if (!isHeld)
                    {
                        if (rb != null)
                        {
                            // Calculate direction back to the center of the minigame
                            Vector3 centerFlat = new Vector3(transform.position.x, 0, transform.position.z);
                            Vector3 messFlat = new Vector3(activeMesses[i].transform.position.x, 0, activeMesses[i].transform.position.z);
                            Vector3 dirToCenter = (centerFlat - messFlat).normalized;

                            // Move it slightly inside the line to prevent it from getting stuck on the edge
                            Vector3 clampedPos = transform.position + (-dirToCenter * (detectionRadius - 0.5f));
                            clampedPos.y = activeMesses[i].transform.position.y;
                            activeMesses[i].transform.position = clampedPos;

                            // Zero out velocity so it doesn't fight the bounce
                            rb.velocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                            
                            // Add force pointing back towards the center, with a slight upward arc
                            Vector3 bounceDir = (dirToCenter + (Vector3.up * 0.5f)).normalized;
                            rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);
                        }
                        else
                        {
                            // If it doesn't have physics (like a flat static texture on the ground), just destroy it
                            Destroy(activeMesses[i]); 
                        }
                    }
                }
            }
        }

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

        if (mess != null && mess.GetComponent<MessDestroyNotifier>() == null)
        {
            mess.AddComponent<MessDestroyNotifier>();
        }
    }

    public void OnMessDestroyed(Vector3 worldPosition)
    {
        if (messDestroyVfxPrefab == null) return;

        ParticleSystem ps = Instantiate(messDestroyVfxPrefab, worldPosition, Quaternion.identity);
        ps.Play();

        var main = ps.main;
        float life = main.duration;
        if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
        {
            life += main.startLifetime.constant;
        }
        else
        {
            life += 1f;
        }

        if (life <= 0f) life = main.duration + 1f;
        Destroy(ps.gameObject, life + 0.1f);
    }

    public void NotifyMessCleaned() 
    {
        if (isGameActive)
        {
            currentPatience += patienceRestorePerClean;
            currentPatience = Mathf.Clamp(currentPatience, 0, maxPatience);
            
            StopCoroutine("AudioGracePeriod"); 
            StartCoroutine(AudioGracePeriod());
        }

        UpdateSuccessUI();
    }

    private IEnumerator AudioGracePeriod()
    {
        isInvulnerableToAudio = true;
        yield return new WaitForSeconds(winGracePeriod);
        isInvulnerableToAudio = false;
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
        if (isInvulnerableToAudio) return; 

        if (voiceInstance.isValid())
        {
            PLAYBACK_STATE pbState;
            voiceInstance.getPlaybackState(out pbState);
            if (pbState == PLAYBACK_STATE.PLAYING) return;
        }

        if (principalAnim != null) principalAnim.Play(hitStateName, 0, 0f); 
        
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
        
        if (SinglePlayerModeManager.Instance != null)
        {
            SinglePlayerModeManager.Instance.SinglePlayerScore += points;
        }

        PlayVoice(3, true); 
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
        
        if (boundaryLine != null) boundaryLine.enabled = false;
        
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

    private class MessDestroyNotifier : MonoBehaviour
    {
        private bool notified = false;

        private void OnDisable()
        {
            NotifyOnce();
        }

        private void OnDestroy()
        {
            NotifyOnce();
        }

        private void NotifyOnce()
        {
            if (notified) return;
            notified = true;

            if (PrincipalMinigame.instance != null)
            {
                PrincipalMinigame.instance.OnMessDestroyed(transform.position);
            }
        }
    }
}