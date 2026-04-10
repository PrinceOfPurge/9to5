using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider))]
public class DirtCleaner : MonoBehaviour, IInteractable
{
    [HideInInspector] 
    public DirtSpawn originSpawnPoint;
    
    public static int DifficultyLevel = 1;

    [Header("Interaction Settings")]
    public int points = 50;
    public GameObject cleaningPrompt;
    public KeyCode interactKey = KeyCode.E;
    
    [Tooltip("Base hold time for Level 1. Automatically scales down by 20% per level.")]
    public float startingHoldTime = 1.2f; 
    [Tooltip("Base margin of error for Level 1.")]
    public float startingPerfectWindow = 0.2f; 
    
    public float fadeSpeed = 1f;
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

    private GameObject crosshair1;
    private GameObject crosshair2;

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
    
    private bool isLookedAt = false; 
    private bool isUIActive = false; 

    private bool miniGameActive = false;
    private float holdTimer = 0f;
    private float currentAlpha = 1f;
    private bool isProcessingResult = false; 

    private Vector3 originalUIScale;
    private Color originalFillColor;
    
    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private Animator playerAnimator;
    private EventInstance mopSoundInstance;

    // Active Difficulty Stats
    private float currentHoldTime;
    private float currentPerfectWindow;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAlpha = sr.color.a;

        if (highlightScript == null)
            highlightScript = GetComponentInChildren<HighlightEffectMultiMesh>();

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerTransform = playerMovement.transform;
            playerAnimator = playerMovement.GetComponentInChildren<Animator>();

            if (playerHandMop == null)
            {
                Transform[] playerChildren = playerMovement.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in playerChildren)
                {
                    if (child.name == "NewMop") 
                    {
                        playerHandMop = child.gameObject;
                        break;
                    }
                }
            }
        }

        if (UIManager.Instance != null)
        {
            crosshair1 = UIManager.Instance.crosshair1;
            crosshair2 = UIManager.Instance.crosshair2;
        }

        if (fillImage != null) originalFillColor = fillImage.color;

        if (miniGameUIParent != null)
        {
            originalUIScale = miniGameUIParent.transform.localScale;
            miniGameUIParent.SetActive(false);
        }

        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
        
        if (crosshair1 != null) crosshair1.SetActive(true);
        if (crosshair2 != null) crosshair2.SetActive(false);

        if (worldMop != null) 
        {
            worldMop.SetActive(true);
            mopStartPos = worldMop.transform.localPosition;
        }
    }

    private void CalculateDifficulty()
    {
        // 20% FASTER PER LEVEL: Multiplies the required hold time by 0.80 each level. Caps at 0.3 seconds.
        currentHoldTime = Mathf.Max(0.3f, startingHoldTime * Mathf.Pow(0.80f, DifficultyLevel - 1));
        
        // TIGHTER WINDOW: Shrinks the perfect window by 15% each level. Caps at 0.04 seconds.
        currentPerfectWindow = Mathf.Max(0.04f, startingPerfectWindow * Mathf.Pow(0.85f, DifficultyLevel - 1));
    }

    private float GetFlatDistanceToPlayer()
    {
        if (playerTransform == null) return 999f;
        Vector3 myPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPosFlat = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        return Vector3.Distance(myPosFlat, playerPosFlat);
    }

    public void OnFocus() 
    { 
        if (miniGameActive) return;
        isLookedAt = true; 
        
        if (GetFlatDistanceToPlayer() <= interactionDistance)
        {
            ToggleInteractionUI(true);
        }
    }

    public void OnLoseFocus()
    {
        isLookedAt = false;
        if (miniGameActive) return;
        ToggleInteractionUI(false); 
    }

    private void ToggleInteractionUI(bool show)
    {
        if (isUIActive == show) return; 
        isUIActive = show;

        if (highlightScript != null) highlightScript.ToggleHighlight(show);
        if (cleaningPrompt != null) cleaningPrompt.SetActive(show);
        
        if (crosshair1 != null) crosshair1.SetActive(!show);
        if (crosshair2 != null) crosshair2.SetActive(show);
    }

    public void OnInteract()
    {
        if (!miniGameActive && isLookedAt && GetFlatDistanceToPlayer() <= interactionDistance) 
            StartMiniGame();
    }

    private void Update()
    {
        if (isLookedAt && !miniGameActive)
        {
            bool inRange = GetFlatDistanceToPlayer() <= interactionDistance;
            if (inRange != isUIActive) 
            {
                ToggleInteractionUI(inRange);
            }
        }

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

        if (Input.GetMouseButtonDown(1)) { CancelMiniGame(); return; }

        if (Input.GetKey(interactKey)) holdTimer += Time.deltaTime;
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(holdTimer / currentHoldTime);

        // Fail if held too long based on dynamic window
        if (holdTimer >= (currentHoldTime + currentPerfectWindow)) StartCoroutine(FailSequence());

        if (Input.GetKeyUp(interactKey))
        {
            float perfectStart = currentHoldTime - currentPerfectWindow;
            float perfectEnd = currentHoldTime + currentPerfectWindow;
            
            if (holdTimer >= perfectStart && holdTimer <= perfectEnd) StartCoroutine(SuccessSequence());
            else StartCoroutine(FailSequence());
        }
    }

    private void StartMiniGame()
    {
        CalculateDifficulty(); // Apply the math right before they start

        miniGameActive = true;
        isProcessingResult = false;
        holdTimer = 0f;
        ToggleInteractionUI(false); 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (crosshair1 != null) crosshair1.SetActive(false);
        if (crosshair2 != null) crosshair2.SetActive(false);
        if (miniGameUIParent != null)
        {
            miniGameUIParent.SetActive(true);
            miniGameUIParent.transform.localScale = originalUIScale; 
        }
        if (miniGamePrompt != null) miniGamePrompt.SetActive(true);
        if (worldMop != null) worldMop.SetActive(false);
        if (playerHandMop != null) playerHandMop.SetActive(true);
        if (playerMovement != null)
        {
            if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
            playerMovement.enabled = false; 
            MinigameFocusManager.Instance.StartFocus(transform, lookOffset, interactionDistance);
        }
    }

    private void CancelMiniGame()
    {
        miniGameActive = false;
        MinigameFocusManager.Instance.StopFocus();
        StopMopSound();
        ResetUIStates();
        if (isLookedAt) ToggleInteractionUI(GetFlatDistanceToPlayer() <= interactionDistance);
        if (worldMop != null) worldMop.SetActive(true);
        if (playerHandMop != null) playerHandMop.SetActive(false);
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
        StopMopSound();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("InteractionActive", false);
            StartCoroutine(FadeLayerWeight(interactionLayerIndex, 0f, 0.3f));
        }
        if (playerHandMop != null) playerHandMop.SetActive(false);
        ResetUIStates();
        if (originSpawnPoint != null) originSpawnPoint.isSpawned = false;
        
        if (SinglePlayerModeManager.Instance != null)
        {
            SinglePlayerModeManager.Instance.BagsRemaining--;
            SinglePlayerModeManager.Instance.SinglePlayerScore += points;
        }

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
        if (crosshair1 != null) crosshair1.SetActive(true);
        if (crosshair2 != null) crosshair2.SetActive(false);
        if (miniGameUIParent != null) miniGameUIParent.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
    }
}