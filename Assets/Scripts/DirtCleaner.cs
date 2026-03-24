using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider))]
public class DirtCleaner : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    public GameObject cleaningPrompt;
    public KeyCode interactKey = KeyCode.E;
    public float holdTime = 2f;
    public float fadeSpeed = 1f;
    public float perfectWindow = 0.3f;
    public GameObject doneVFX;
    public int interactionLayerIndex = 1;

    [Header("Highlighting")]
    public HighlightEffectMultiMesh highlightScript; 

    [Header("Positioning & Alignment")]
    public float interactionDistance = 1.8f; 
    public Vector3 lookOffset = new Vector3(0, -0.5f, 0); 

    [Header("UI (World Space)")]
    public GameObject miniGameUIParent;
    public Image fillImage;
    public GameObject miniGamePrompt;

    [Header("Cursor")]
    public Image cursorUI;
    public Sprite defaultCursorSprite;
    public Sprite interactCursorSprite;

    [Header("Flash & Feedback")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float rewindSpeed = 2.5f;
    public float successPulseAmount = 1.2f;

    [Header("World Mop")]
    public GameObject worldMop;
    public float mopRotationSpeed = 180f;
    public float mopFloatHeight = 0.1f;
    public float mopFloatSpeed = 2f;

    [Header("Player Hand Mop")]
    public GameObject playerHandMop;

    private Vector3 mopStartPos;
    private SpriteRenderer sr;
    private bool playerInRange = false;
    private bool miniGameActive = false;
    private float holdTimer = 0f;
    private float currentAlpha = 1f;
    private bool isProcessingResult = false; 

    private Vector3 originalUIScale;
    private Color originalFillColor;
    private PlayerMovement playerMovement;
    private Animator playerAnimator;
    private EventInstance mopSoundInstance;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAlpha = sr.color.a;

        if (fillImage != null) originalFillColor = fillImage.color;

        if (miniGameUIParent != null)
        {
            originalUIScale = miniGameUIParent.transform.localScale;
            miniGameUIParent.SetActive(false);
        }

        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
        if (playerHandMop != null) playerHandMop.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;

        if (worldMop != null) mopStartPos = worldMop.transform.localPosition;
    }

    public void OnFocus()
    {
        if (miniGameActive) return;
        playerInRange = true;
        if (highlightScript != null) highlightScript.ToggleHighlight(true);
        if (cleaningPrompt != null) cleaningPrompt.SetActive(true);
        if (cursorUI != null && interactCursorSprite != null) cursorUI.sprite = interactCursorSprite;
    }

    public void OnLoseFocus()
    {
        if (miniGameActive) return;
        playerInRange = false;
        if (highlightScript != null) highlightScript.ToggleHighlight(false);
        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (cursorUI != null && defaultCursorSprite != null) cursorUI.sprite = defaultCursorSprite;
    }

    public void OnInteract()
    {
        if (!miniGameActive && playerInRange) StartMiniGame();
    }

    private void Update()
    {
        if (miniGameActive && Time.timeScale == 0)
        {
            CancelMiniGame();
            return;
        }

        if (worldMop != null && worldMop.activeSelf)
        {
            worldMop.transform.Rotate(Vector3.up * mopRotationSpeed * Time.deltaTime, Space.World);
            float newY = mopStartPos.y + Mathf.Sin(Time.time * mopFloatSpeed) * mopFloatHeight;
            worldMop.transform.localPosition = new Vector3(worldMop.transform.localPosition.x, newY, worldMop.transform.localPosition.z);
        }

        if (!miniGameActive || isProcessingResult) return;

        if (Input.GetMouseButtonDown(1))
        {
            CancelMiniGame();
            return;
        }

        if (Input.GetKey(interactKey)) holdTimer += Time.deltaTime;
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTime);

        if (holdTimer >= (holdTime + perfectWindow)) StartCoroutine(FailSequence());

        if (Input.GetKeyUp(interactKey))
        {
            float perfectStart = holdTime - perfectWindow;
            float perfectEnd = holdTime + perfectWindow;

            if (holdTimer >= perfectStart && holdTimer <= perfectEnd)
                StartCoroutine(SuccessSequence());
            else
                StartCoroutine(FailSequence());
        }
    }

    private void StartMiniGame()
    {
        miniGameActive = true;
        isProcessingResult = false;
        holdTimer = 0f;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cursorUI != null) cursorUI.enabled = false; 

        if (highlightScript != null) highlightScript.ToggleHighlight(false);

        if (miniGameUIParent != null)
        {
            miniGameUIParent.SetActive(true);
            miniGameUIParent.transform.localScale = originalUIScale; 
        }

        if (miniGamePrompt != null) miniGamePrompt.SetActive(true);
        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (worldMop != null) worldMop.SetActive(false);

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerAnimator = playerMovement.GetComponentInChildren<Animator>();
            
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0f);
            }

            playerMovement.enabled = false; 
            
            // The MinigameFocusManager now handles the distance alignment!
            MinigameFocusManager.Instance.StartFocus(transform, lookOffset, interactionDistance);
        }
    }

    private void CancelMiniGame()
    {
        miniGameActive = false;
        MinigameFocusManager.Instance.StopFocus();
        
        // --- REMOVED: playerMovement.enabled = true; ---
        
        StopMopSound();
        ResetUIStates();
        if (worldMop != null) worldMop.SetActive(true);
    }

    private IEnumerator SuccessSequence()
    {
        isProcessingResult = true;
        if (fillImage != null) fillImage.color = successColor;
        miniGameUIParent.transform.localScale = originalUIScale * successPulseAmount;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("InteractionActive", true);
            StartCoroutine(FadeLayerWeight(interactionLayerIndex, 1f, 0.2f));
            
            mopSoundInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.Broom);
            mopSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            mopSoundInstance.start();

            if (playerHandMop != null) playerHandMop.SetActive(true);
        }

        yield return new WaitForSeconds(0.2f);

        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            miniGameUIParent.transform.localScale = Vector3.Lerp(originalUIScale * successPulseAmount, Vector3.zero, t / 0.4f);
            yield return null;
        }

        miniGameUIParent.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
        StartCoroutine(FadeDirt());
    }

    private IEnumerator FailSequence()
    {
        isProcessingResult = true;
        if (fillImage != null) fillImage.color = failColor;
        Vector3 originalLocalPos = miniGameUIParent.transform.localPosition;
        
        for (int i = 0; i < 8; i++)
        {
            miniGameUIParent.transform.localPosition = originalLocalPos + (Random.insideUnitSphere * 0.05f); 
            yield return new WaitForSeconds(0.015f);
        }
        miniGameUIParent.transform.localPosition = originalLocalPos;

        float startFill = fillImage != null ? fillImage.fillAmount : 0;
        float elapsed = 0f;
        while (fillImage != null && fillImage.fillAmount > 0)
        {
            elapsed += Time.deltaTime * rewindSpeed;
            fillImage.fillAmount = Mathf.Lerp(startFill, 0, elapsed);
            yield return null;
        }

        if (fillImage != null) fillImage.color = originalFillColor;
        holdTimer = 0f;
        isProcessingResult = false; 
    }

    private IEnumerator FadeDirt()
    {
        while (currentAlpha > 0f)
        {
            currentAlpha -= fadeSpeed * Time.deltaTime;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Clamp01(currentAlpha));
            yield return null;
        }
        FinishMiniGame();
    }

    private void FinishMiniGame()
    {
        miniGameActive = false;
        MinigameFocusManager.Instance.StopFocus();
        
        // --- REMOVED: playerMovement.enabled = true; ---
        
        StopMopSound();
        
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("InteractionActive", false);
            StartCoroutine(FadeLayerWeight(interactionLayerIndex, 0f, 0.3f));
        }
        
        if (playerHandMop != null) playerHandMop.SetActive(false);
        ResetUIStates();
        
        if (doneVFX != null) Destroy(Instantiate(doneVFX, transform.position, Quaternion.identity), 2f);
        Destroy(gameObject);
    }

    private void StopMopSound()
    {
        mopSoundInstance.getPlaybackState(out PLAYBACK_STATE state);
        if (state != PLAYBACK_STATE.STOPPED)
        {
            mopSoundInstance.stop(STOP_MODE.ALLOWFADEOUT);
            mopSoundInstance.release();
        }
    }
    
    private IEnumerator FadeLayerWeight(int index, float target, float duration)
    {
        if (playerAnimator == null) yield break;
        float start = playerAnimator.GetLayerWeight(index);
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerAnimator.SetLayerWeight(index, Mathf.Lerp(start, target, elapsed / duration));
            yield return null;
        }
        playerAnimator.SetLayerWeight(index, target);
    }

    private void ResetUIStates()
    {
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;
        if (cursorUI != null)
        {
            cursorUI.enabled = true;
            cursorUI.sprite = defaultCursorSprite;
        }
        if (miniGameUIParent != null) miniGameUIParent.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
    }
}