using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Banana : MonoBehaviour, IInteractable
{
    public static Banana ActiveBanana = null;
    public static int DifficultyLevel = 1;

    [Header("Interaction")]
    public GameObject garbagePrompt;
    public KeyCode interactKey = KeyCode.E;
    public int points = 100;

    [Header("Positioning")]
    public float interactionDistance = 1.5f; 
    public Vector3 lookOffset = new Vector3(0, 0, 0);

    [Header("UI Crosshair (Standard System)")]
    public GameObject crosshair1; 
    public GameObject crosshair2;

    [Header("Minigame UI")]
    public GameObject miniGameUIParent; 
    public Image upArrowUI;
    public Image downArrowUI;
    public Image leftArrowUI;
    public Image rightArrowUI;
    public Transform uiLocation; 

    [Header("Animation Settings")]
    public float pulseScale = 1.3f;
    public float pulseSpeed = 10f;
    public float shakeIntensity = 0.05f;

    [Header("Feedback")]
    public float correctFlashTime = 0.15f;
    public float wrongFlashTime = 0.15f;
    
    // Kept original name so Inspector doesn't break
    public int totalKeysNeeded = 6;

    [Header("Effects")]
    public GameObject doneVFX;
    public GameObject Bananas;
    public HighlightEffectBananaAndGarbage highlightScript;

    [Header("Timer")]
    // Kept original name so Inspector doesn't break
    public float maxTime = 10f;
    public Image timerBarUI;
    public GameObject timerUI;

    private bool playerInRange;
    private bool isCleaned;
    private bool isProcessingAnimation = false;
    private bool isThisBananaActive = false; 

    private float timer;
    private KeyCode currentKey;
    private int remainingKeys;
    private bool ignoreInputThisFrame;

    // Active Difficulty Stats (calculated quietly)
    private int currentTotalKeysNeeded;
    private float currentMaxTime;

    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
    private PlayerMovement playerMovement;

    private KeyCode[] keyPool = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    private void Start()
    {
        PrincipalMinigame principal = FindObjectOfType<PrincipalMinigame>();
        if (principal != null)
        {
            crosshair1 = principal.defaultCursorObj;
            crosshair2 = principal.interactCursorObj;
        }

        if (garbagePrompt != null) garbagePrompt.SetActive(false);
        HideAllArrows();

        if (upArrowUI != null) originalColors[upArrowUI] = upArrowUI.color;
        if (downArrowUI != null) originalColors[downArrowUI] = downArrowUI.color;
        if (leftArrowUI != null) originalColors[leftArrowUI] = leftArrowUI.color;
        if (rightArrowUI != null) originalColors[rightArrowUI] = rightArrowUI.color;

        if (timerUI != null) timerUI.SetActive(false);
    }

    private void CalculateDifficulty()
    {
        // Add 1 extra key sequence per level
        currentTotalKeysNeeded = totalKeysNeeded + (DifficultyLevel - 1);
        
        // Subtract 0.5 seconds from the timer per level (but never let it drop below 3.5 seconds)
        currentMaxTime = Mathf.Max(3.5f, maxTime - ((DifficultyLevel - 1) * 0.5f));
    }

    public void OnFocus()
    {
        if (ActiveBanana != null || isCleaned) return;
        playerInRange = true;
        
        if (highlightScript != null) highlightScript.ToggleHighlight(true);
        if (garbagePrompt != null) garbagePrompt.SetActive(true);
        
        if (crosshair1 != null) crosshair1.SetActive(false);
        if (crosshair2 != null) crosshair2.SetActive(true);
    }

    public void OnLoseFocus()
    {
        if (isThisBananaActive) return;
        playerInRange = false;
        
        if (highlightScript != null) highlightScript.ToggleHighlight(false);
        if (garbagePrompt != null) garbagePrompt.SetActive(false);
    
        if (crosshair1 != null) crosshair1.SetActive(true);
        if (crosshair2 != null) crosshair2.SetActive(false);
    }

    public void OnInteract()
    {
        if (ActiveBanana == null && playerInRange && !isCleaned) 
        {
            StartMiniGame();
        }
    }

    private void Update()
    {
        
        if (!isThisBananaActive) return;

        if (Time.timeScale == 0)
        {
            CancelMiniGame();
            return;
        }

        if (isCleaned || isProcessingAnimation) return;
        
        if (ignoreInputThisFrame) 
        { 
            ignoreInputThisFrame = false; 
            return; 
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMiniGame();
            return;
        }

        timer -= Time.deltaTime;
        
        // Uses the scaled time
        if (timerBarUI != null) timerBarUI.fillAmount = timer / currentMaxTime;

        if (timer <= 0f) 
        { 
            StartCoroutine(HandleWrong()); 
            return; 
        }

        if (Input.GetKeyDown(currentKey))
        {
            StartCoroutine(HandleCorrect());
        }
        else if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                StartCoroutine(HandleWrong());
            }
        }
    }

    private void StartMiniGame()
    {
        CalculateDifficulty(); // Apply the math right before they start

        ActiveBanana = this; // Set the global lock
        isThisBananaActive = true;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (crosshair1 != null) crosshair1.SetActive(false);
        if (crosshair2 != null) crosshair2.SetActive(false);

        if (highlightScript != null) highlightScript.ToggleHighlight(false);
        if (garbagePrompt != null) garbagePrompt.SetActive(false);

        if (miniGameUIParent != null) miniGameUIParent.SetActive(true);
        if (timerUI != null) timerUI.SetActive(true);

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            Animator anim = playerMovement.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetFloat("Speed", 0f); 

            playerMovement.enabled = false;
            
            Transform targetTransform = uiLocation != null ? uiLocation : transform;
            MinigameFocusManager.Instance.StartFocus(targetTransform, lookOffset, interactionDistance);
        }

        // Uses the scaled values
        remainingKeys = currentTotalKeysNeeded;
        timer = currentMaxTime;
        ShowRandomKey();
    }

    private void CancelMiniGame()
    {
        isThisBananaActive = false;
        if (ActiveBanana == this) ActiveBanana = null; // Release the lock
        
        MinigameFocusManager.Instance.StopFocus();
        
        RestoreAllArrowColorsToOriginal();
        ResetUIStates();
    }

    private void FinishMiniGame()
    {
        isThisBananaActive = false;
        if (ActiveBanana == this) ActiveBanana = null; // Release the lock
        
        isCleaned = true;
        MinigameFocusManager.Instance.StopFocus();
        
        ResetUIStates();
        
        PrincipalMinigame principal = FindObjectOfType<PrincipalMinigame>();
        if (principal != null) principal.NotifyMessCleaned();
        
        SinglePlayerModeManager.Instance.SinglePlayerScore += points;
        SinglePlayerModeManager.Instance.BagsRemaining--;

        if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);
        if (doneVFX != null) Destroy(Instantiate(doneVFX, transform.position, Quaternion.identity), 2f);
        if (Bananas != null) Destroy(Bananas);
        Destroy(gameObject); 
    }

    private void ResetUIStates()
    {
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;
        
        if (crosshair1 != null) crosshair1.SetActive(true);
        if (crosshair2 != null) crosshair2.SetActive(false);
        
        if (miniGameUIParent != null) miniGameUIParent.SetActive(false);
        if (timerUI != null) timerUI.SetActive(false);
        HideAllArrows();
    }

    private IEnumerator HandleCorrect()
    {
        isProcessingAnimation = true;
        
        if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Success, transform.position);

        Image img = GetArrowImage(currentKey);
        
        if (img != null)
        {
            img.color = Color.green;
            Vector3 startScale = img.transform.localScale;
            Vector3 targetScale = startScale * pulseScale;
            float t = 0;
            while (t < 1.0f)
            {
                t += Time.deltaTime * pulseSpeed;
                img.transform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            img.transform.localScale = startScale;
            img.color = originalColors[img];
        }

        remainingKeys--;
        isProcessingAnimation = false;
        
        if (remainingKeys <= 0) 
            FinishMiniGame();
        else 
            ShowRandomKey();
    }

    private IEnumerator HandleWrong()
    {
        isProcessingAnimation = true;

        if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Fail, transform.position);

        Image img = GetArrowImage(currentKey);
        Vector3 originalPos = miniGameUIParent != null ? miniGameUIParent.transform.localPosition : Vector3.zero;

        if (img != null) img.color = Color.red;

        for (int i = 0; i < 8; i++)
        {
            if (miniGameUIParent != null)
                miniGameUIParent.transform.localPosition = originalPos + (Random.insideUnitSphere * shakeIntensity);
            yield return new WaitForSeconds(0.02f);
        }

        if (miniGameUIParent != null) miniGameUIParent.transform.localPosition = originalPos;
        if (img != null) img.color = originalColors[img];

        // Reset using scaled values
        remainingKeys = currentTotalKeysNeeded;
        timer = currentMaxTime;
        
        isProcessingAnimation = false;
        ShowRandomKey();
    }

    void HideAllArrows() 
    { 
        if (upArrowUI != null) upArrowUI.gameObject.SetActive(false); 
        if (downArrowUI != null) downArrowUI.gameObject.SetActive(false); 
        if (leftArrowUI != null) leftArrowUI.gameObject.SetActive(false); 
        if (rightArrowUI != null) rightArrowUI.gameObject.SetActive(false); 
    }
    
    void ShowRandomKey() 
    { 
        HideAllArrows(); 
        RestoreAllArrowColorsToOriginal(); 
        currentKey = keyPool[Random.Range(0, keyPool.Length)]; 
        Image img = GetArrowImage(currentKey); 
        if (img != null) img.gameObject.SetActive(true); 
    }
    
    Image GetArrowImage(KeyCode k) 
    { 
        switch (k) 
        { 
            case KeyCode.W: return upArrowUI; 
            case KeyCode.S: return downArrowUI; 
            case KeyCode.A: return leftArrowUI; 
            case KeyCode.D: return rightArrowUI; 
        } 
        return null; 
    }
    
    void RestoreAllArrowColorsToOriginal() 
    { 
        foreach (var kv in originalColors) 
        { 
            if (kv.Key != null) kv.Key.color = kv.Value; 
        } 
    }
}