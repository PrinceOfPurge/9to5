using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class Banana : MonoBehaviour, IInteractable
{
    public static bool isMinigameActive = false;

    [Header("Interaction")]
    public GameObject garbagePrompt;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Crosshair (Standard System)")]
    public GameObject crosshair1; 
    public GameObject crosshair2;

    [Header("Minigame UI")]
    public Image upArrowUI;
    public Image downArrowUI;
    public Image leftArrowUI;
    public Image rightArrowUI;

    [Header("Feedback")]
    public float correctFlashTime = 0.15f;
    public float wrongFlashTime = 0.15f;
    public int totalKeysNeeded = 6;
    public Renderer mainColor;

    [Header("Effects")]
    public GameObject doneVFX;
    public GameObject Bananas;

    [Header("Timer")]
    public float maxTime = 10f;
    public Image timerBarUI;
    public GameObject timerUI;

    private bool playerInRange;
    private bool isPlaying;
    private bool isCleaned;

    private float timer;
    private KeyCode currentKey;
    private int remainingKeys;
    private bool ignoreInputThisFrame;

    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
    private PlayerMovement playerMovement;

    private KeyCode[] keyPool = new KeyCode[]
    {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D
    };

    private void Awake()
    {
        garbagePrompt?.SetActive(false);
        HideAllArrows();

        if (upArrowUI != null) originalColors[upArrowUI] = upArrowUI.color;
        if (downArrowUI != null) originalColors[downArrowUI] = downArrowUI.color;
        if (leftArrowUI != null) originalColors[leftArrowUI] = leftArrowUI.color;
        if (rightArrowUI != null) originalColors[rightArrowUI] = rightArrowUI.color;

        remainingKeys = totalKeysNeeded;

        if (timerUI != null)
            timerUI.SetActive(false);
    }

    // --- INTERFACE METHODS ---

    public void OnFocus()
    {
        if (isCleaned || isPlaying) return;

        playerInRange = true;

        // Visual feedback for "Looking At"
        if (crosshair1) crosshair1.SetActive(false);
        if (crosshair2) crosshair2.SetActive(true);
        if (garbagePrompt) garbagePrompt.SetActive(true);
    }

    public void OnLoseFocus()
    {
        playerInRange = false;

        // If player looks away while playing, cancel minigame
        if (isPlaying)
            EndMinigame(false);

        // Reset visual feedback
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);
        if (garbagePrompt) garbagePrompt.SetActive(false);
    }

    public void OnInteract()
    {
        if (playerInRange && !isPlaying && !isCleaned)
        {
            StartMinigame();
        }
    }

    // --- LOGIC ---

    private void Update()
    {
        if (isCleaned || !isPlaying) return;

        if (ignoreInputThisFrame)
        {
            ignoreInputThisFrame = false;
            return;
        }

        timer -= Time.deltaTime;

        if (timerBarUI != null)
            timerBarUI.fillAmount = timer / maxTime;

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

    void StartMinigame()
    {
        isPlaying = true;
        isMinigameActive = true;

        playerMovement = GameObject.FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false; 

        remainingKeys = totalKeysNeeded;
        if (garbagePrompt) garbagePrompt.SetActive(false);

        ignoreInputThisFrame = true;
        timer = maxTime;

        if (timerUI != null)
            timerUI.SetActive(true);

        ShowRandomKey();
    }

    void EndMinigame(bool completed)
    {
        isPlaying = false;
        isMinigameActive = false;

        if (playerMovement != null)
            playerMovement.enabled = true; 

        if (timerUI != null)
            timerUI.SetActive(false);

        HideAllArrows();
        
        // Final cleanup of UI
        if (garbagePrompt) garbagePrompt.SetActive(false);
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);

        if (completed)
        {
            isCleaned = true;
            if (mainColor != null) mainColor.material.color = Color.green;
            if (AudioManager.instance) AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);

            if (doneVFX != null)
                Destroy(Instantiate(doneVFX, transform.position, Quaternion.identity), 2f);

            if (Bananas != null) Destroy(Bananas);
            Destroy(this); // Optional: removes script so you can't interact again
        }
        else
        {
            RestoreAllArrowColorsToOriginal();
        }
    }

    void HideAllArrows()
    {
        upArrowUI?.gameObject.SetActive(false);
        downArrowUI?.gameObject.SetActive(false);
        leftArrowUI?.gameObject.SetActive(false);
        rightArrowUI?.gameObject.SetActive(false);
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

    private IEnumerator HandleCorrect()
    {
        Image img = GetArrowImage(currentKey);
        if (img != null)
        {
            Color original = img.color;
            img.color = Color.green;
            yield return new WaitForSeconds(correctFlashTime);
            img.color = original;
        }

        remainingKeys--;
        if (remainingKeys <= 0) EndMinigame(true);
        else ShowRandomKey();
    }

    private IEnumerator HandleWrong()
    {
        Image img = GetArrowImage(currentKey);
        if (img != null)
        {
            Color original = img.color;
            img.color = Color.red;
            yield return new WaitForSeconds(wrongFlashTime);
            img.color = original;
        }

        remainingKeys = totalKeysNeeded;
        timer = maxTime;
        ShowRandomKey();
    }
}