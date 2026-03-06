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

    [Header("Positioning")]
    [Tooltip("Ideal distance for the player to stand from the banana")]
    public float interactionDistance = 1.5f; 
    public float positioningSpeed = 5f;

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

    [Header("Camera Lock Settings")]
    public float cameraLockSpeed = 5f;
    public Vector3 lookOffset = new Vector3(0, 0, 0);

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
    private Coroutine cameraLockCoroutine;
    private Coroutine positioningCoroutine; // Added

    private KeyCode[] keyPool = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    private void Awake()
    {
        playerCam = Camera.main;
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
        if (isPlaying) EndMinigame(false);
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);
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

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            
            // 1. Lock Camera
            if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
            cameraLockCoroutine = StartCoroutine(LockCameraToUI());

            // 2. Smoothly Move Player to the ideal distance (NEW)
            if (positioningCoroutine != null) StopCoroutine(positioningCoroutine);
            positioningCoroutine = StartCoroutine(MovePlayerToInteractPoint());
        }

        remainingKeys = totalKeysNeeded;
        timer = maxTime;
        if (timerUI != null) timerUI.SetActive(true);
        ShowRandomKey();
    }

    // NEW: Moves player to the correct interaction distance
    private IEnumerator MovePlayerToInteractPoint()
    {
        while (isPlaying)
        {
            Vector3 targetPos = transform.position;
            Vector3 playerPos = playerMovement.transform.position;
            
            Vector3 dirToPlayer = (playerPos - targetPos).normalized;
            dirToPlayer.y = 0; 

            Vector3 finalTarget = targetPos + (dirToPlayer * interactionDistance);
            Vector3 moveDiff = finalTarget - playerMovement.transform.position;
            
            if (moveDiff.magnitude > 0.05f)
            {
                CharacterController cc = playerMovement.GetComponent<CharacterController>();
                if(cc != null) cc.Move(moveDiff * Time.deltaTime * positioningSpeed);
            }
            yield return null;
        }
    }

    private IEnumerator LockCameraToUI()
    {
        Transform target = uiLocation != null ? uiLocation : transform;
        while (isPlaying)
        {
            if (Time.timeScale > 0)
            {
                Vector3 targetPos = target.position + lookOffset;
                Vector3 direction = (targetPos - playerCam.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    playerMovement.transform.rotation = Quaternion.Slerp(playerMovement.transform.rotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), Time.deltaTime * cameraLockSpeed);
                    float targetX = lookRotation.eulerAngles.x;
                    if (targetX > 180) targetX -= 360;
                    playerCam.transform.localRotation = Quaternion.Slerp(playerCam.transform.localRotation, Quaternion.Euler(targetX, 0, 0), Time.deltaTime * cameraLockSpeed);
                }
            }
            yield return null;
        }
    }

    void EndMinigame(bool completed)
    {
        isPlaying = false;
        isMinigameActive = false;

        // Stop positioning coroutine
        if (positioningCoroutine != null) StopCoroutine(positioningCoroutine);

        if (playerMovement != null)
        {
            playerMovement.SyncRotation(playerCam.transform.localRotation.eulerAngles.x);
            playerMovement.enabled = true;
        }

        if (timerUI != null) timerUI.SetActive(false);
        HideAllArrows();
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);

        if (completed)
        {
            isCleaned = true;
            if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);
            if (doneVFX != null) Destroy(Instantiate(doneVFX, transform.position, Quaternion.identity), 2f);
            if (Bananas != null) Destroy(Bananas);
            Destroy(this);
        }
        else
        {
            RestoreAllArrowColorsToOriginal();
        }
    }

    // --- UTILITIES ---
    private IEnumerator HandleCorrect()
    {
        isProcessingAnimation = true;
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