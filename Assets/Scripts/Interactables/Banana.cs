using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class Banana : MonoBehaviour
{
    // Global flag (optional, useful if other systems need it)
    public static bool isMinigameActive = false;

    [Header("Interaction")]
    public GameObject garbagePrompt;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Cursor")]
    public Image cursorUI;
    public Sprite defaultCursorSprite;
    public Sprite interactCursorSprite;

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

    // Internal state
    private bool playerInRange;
    private bool isPlaying;
    private bool isCleaned;

    private float timer;
    private KeyCode currentKey;
    private int remainingKeys;
    private bool ignoreInputThisFrame;

    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();

    // 🔑 Player movement reference
    private PlayerMovement playerMovement;

    // WASD key pool
    private KeyCode[] keyPool = new KeyCode[]
    {
        KeyCode.W,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D
    };

    private void Awake()
    {
        garbagePrompt?.SetActive(false);
        HideAllArrows();

        if (cursorUI != null)
            cursorUI.sprite = defaultCursorSprite;

        if (upArrowUI != null) originalColors[upArrowUI] = upArrowUI.color;
        if (downArrowUI != null) originalColors[downArrowUI] = downArrowUI.color;
        if (leftArrowUI != null) originalColors[leftArrowUI] = leftArrowUI.color;
        if (rightArrowUI != null) originalColors[rightArrowUI] = rightArrowUI.color;

        remainingKeys = totalKeysNeeded;

        if (timerUI != null)
            timerUI.SetActive(false);
    }

    private void Update()
    {
        if (isCleaned) return;

        // Start minigame
        if (playerInRange && !isPlaying && Input.GetKeyDown(interactKey))
        {
            StartMinigame();
            return;
        }

        if (!isPlaying) return;

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
            if (Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.D))
            {
                StartCoroutine(HandleWrong());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isCleaned) return;
        if (!other.CompareTag("MainCamera")) return;

        playerInRange = true;

        // Cache PlayerMovement once
        if (playerMovement == null)
            playerMovement = other.GetComponentInParent<PlayerMovement>();

        if (!isPlaying)
            garbagePrompt?.SetActive(true);

        if (cursorUI != null && interactCursorSprite != null)
            cursorUI.sprite = interactCursorSprite;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;

        playerInRange = false;

        if (isPlaying)
            EndMinigame(false);

        garbagePrompt?.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;
    }

    void StartMinigame()
    {
        isPlaying = true;
        isMinigameActive = true;

        if (playerMovement != null)
            playerMovement.enabled = false; // 🚫 disable movement

        remainingKeys = totalKeysNeeded;
        garbagePrompt?.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;

        ignoreInputThisFrame = true;
        timer = maxTime;

        if (timerUI != null)
            timerUI.SetActive(true);

        if (timerBarUI != null)
            timerBarUI.fillAmount = 1f;

        ShowRandomKey();
    }

    void EndMinigame(bool completed)
    {
        isPlaying = false;
        isMinigameActive = false;

        if (playerMovement != null)
            playerMovement.enabled = true; // ✅ re-enable movement

        if (timerUI != null)
            timerUI.SetActive(false);

        HideAllArrows();
        garbagePrompt?.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;

        if (completed)
        {
            isCleaned = true;

            if (mainColor != null)
                mainColor.material.color = Color.green;

            AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);

            if (doneVFX != null)
            {
                GameObject vfxObj = Instantiate(doneVFX, transform.position, Quaternion.identity);
                Destroy(vfxObj, 2f);
            }

            Destroy(Bananas);
        }
        else
        {
            RestoreAllArrowColorsToOriginal();
            if (timerBarUI != null) timerBarUI.fillAmount = 1f;
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

        if (img != null)
            img.gameObject.SetActive(true);
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
            if (kv.Key != null)
                kv.Key.color = kv.Value;
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

        if (remainingKeys <= 0)
        {
            EndMinigame(true);
            yield break;
        }

        ShowRandomKey();
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

        if (timerBarUI != null)
            timerBarUI.fillAmount = 1f;

        yield return null;
        ShowRandomKey();
    }
}
