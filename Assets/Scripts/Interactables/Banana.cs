using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Banana : MonoBehaviour, IInteractable
{
    public static bool isMinigameActive = false;

    [Header("Interaction")]
    public GameObject garbagePrompt;
    public KeyCode interactKey = KeyCode.E;
    public int points = 100;

    [Header("Positioning")]
    [Tooltip("Ideal distance for the player to stand from the banana")]
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
    public int totalKeysNeeded = 6;

    [Header("Effects")]
    public GameObject doneVFX;
    public GameObject Bananas;
    public HighlightEffectBananaAndGarbage highlightScript;

    [Header("Timer")]
    public float maxTime = 10f;
    public Image timerBarUI;
    public GameObject timerUI;

    private bool playerInRange;
    private bool isPlaying;
    private bool isCleaned;
    private bool isProcessingAnimation = false;

    private float timer;
    private KeyCode currentKey;
    private int remainingKeys;
    private bool ignoreInputThisFrame;

    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
    private PlayerMovement playerMovement;
    private Camera playerCam;

    private KeyCode[] keyPool = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    private void Awake()
    {
        playerCam = Camera.main;

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

    public void OnFocus()
    {
        if (isCleaned || isPlaying) return;
        playerInRange = true;

        if (highlightScript) highlightScript.ToggleHighlight(true);
        
        if (crosshair1) crosshair1.SetActive(false);
        if (crosshair2) crosshair2.SetActive(true);
    
        if (garbagePrompt) garbagePrompt.SetActive(true);
    }

    public void OnLoseFocus()
    {
        playerInRange = false;
        if (highlightScript) highlightScript.ToggleHighlight(false);
    
        if (!isPlaying)
        {
            if (crosshair1) crosshair1.SetActive(true);
            if (crosshair2) crosshair2.SetActive(false);
        }

        if (garbagePrompt) garbagePrompt.SetActive(false);
    }

    public void OnInteract()
    {
        if (playerInRange && !isPlaying && !isCleaned) StartMinigame();
    }

    private void Update()
    {
        if (isPlaying && Time.timeScale == 0)
        {
            EndMinigame(false);
            return;
        }

        if (isCleaned || !isPlaying || isProcessingAnimation) return;
        if (ignoreInputThisFrame) { ignoreInputThisFrame = false; return; }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            EndMinigame(false);
            return;
        }

        timer -= Time.deltaTime;
        if (timerBarUI != null) timerBarUI.fillAmount = timer / maxTime;

        if (timer <= 0f) { StartCoroutine(HandleWrong()); return; }

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

    void StartMinigame()
    {
        isPlaying = true;
        isMinigameActive = true;
        
        if (highlightScript) highlightScript.ToggleHighlight(false);
        if (garbagePrompt != null) garbagePrompt.SetActive(false);
        if (crosshair1) crosshair1.SetActive(false);
        if (crosshair2) crosshair2.SetActive(false);

        if (miniGameUIParent != null) miniGameUIParent.SetActive(true);

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            Animator anim = playerMovement.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f); 
            }

            playerMovement.enabled = false;
            
            // Hand off to the Focus Manager
            Transform targetTransform = uiLocation != null ? uiLocation : transform;
            MinigameFocusManager.Instance.StartFocus(targetTransform, lookOffset, interactionDistance);
        }

        remainingKeys = totalKeysNeeded;
        timer = maxTime;
        if (timerUI != null) timerUI.SetActive(true);
        ShowRandomKey();
    }

    void EndMinigame(bool completed)
    {
        isPlaying = false;
        isMinigameActive = false;

        // Trigger the smooth exit in the Focus Manager
        MinigameFocusManager.Instance.StopFocus();

        if (miniGameUIParent != null) miniGameUIParent.SetActive(false);

        // --- REMOVED: playerMovement.enabled = true; ---

        if (timerUI != null) timerUI.SetActive(false);
        HideAllArrows();
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);

        if (completed)
        {
            isCleaned = true;

            PrincipalMinigame principal = FindObjectOfType<PrincipalMinigame>();
            if (principal != null)
            {
                principal.NotifyMessCleaned();
            }
            SinglePlayerModeManager.Instance.SinglePlayerScore += points;
            SinglePlayerModeManager.Instance.BagsRemaining--;

            if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);
            if (doneVFX != null) Destroy(Instantiate(doneVFX, transform.position, Quaternion.identity), 2f);
            if (Bananas != null) Destroy(Bananas);
            Destroy(gameObject); 
        }
        else
        {
            RestoreAllArrowColorsToOriginal();
        }
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
        if (remainingKeys <= 0) EndMinigame(true);
        else ShowRandomKey();
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

        remainingKeys = totalKeysNeeded;
        timer = maxTime;
        isProcessingAnimation = false;
        ShowRandomKey();
    }

    void HideAllArrows() { upArrowUI?.gameObject.SetActive(false); downArrowUI?.gameObject.SetActive(false); leftArrowUI?.gameObject.SetActive(false); rightArrowUI?.gameObject.SetActive(false); }
    void ShowRandomKey() { HideAllArrows(); RestoreAllArrowColorsToOriginal(); currentKey = keyPool[Random.Range(0, keyPool.Length)]; Image img = GetArrowImage(currentKey); if (img != null) img.gameObject.SetActive(true); }
    Image GetArrowImage(KeyCode k) { switch (k) { case KeyCode.W: return upArrowUI; case KeyCode.S: return downArrowUI; case KeyCode.A: return leftArrowUI; case KeyCode.D: return rightArrowUI; } return null; }
    void RestoreAllArrowColorsToOriginal() { foreach (var kv in originalColors) { if (kv.Key != null) kv.Key.color = kv.Value; } }
}