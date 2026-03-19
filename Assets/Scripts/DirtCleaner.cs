using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Highlighting")]
    public HighlightEffectMultiMesh highlightScript; 

    [Header("Positioning")]
    [Tooltip("How far the player should stand from the mess during the game")]
    public float interactionDistance = 1.8f; 
    public float positioningSpeed = 5f;

    [Header("UI (World Space)")]
    public GameObject miniGameUIParent;
    public Image fillImage;
    public Image sweetSpotMarker;
    public Vector3 lookOffset = new Vector3(0, -0.5f, 0); 

    [Header("Mini Game Prompt")]
    public GameObject miniGamePrompt;

    [Header("Cursor")]
    public Image cursorUI;
    public Sprite defaultCursorSprite;
    public Sprite interactCursorSprite;

    [Header("Flash & Feedback")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float flashDuration = 0.15f;
    public float rewindSpeed = 2.5f;
    public float successPulseAmount = 1.2f;

    [Header("World Mop (Pickup)")]
    public GameObject worldMop;
    public float mopRotationSpeed = 180f;
    public float mopFloatHeight = 0.1f;
    public float mopFloatSpeed = 2f;

    [Header("Player Hand Mop")]
    public GameObject playerHandMop;

    [Header("Camera Lock")]
    public float cameraLockSpeed = 5f;

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
    private Camera playerCam;
    private Coroutine cameraLockCoroutine;
    private Coroutine positioningCoroutine;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAlpha = sr.color.a;
        playerCam = Camera.main;

        if (fillImage != null) originalFillColor = fillImage.color;

        if (miniGameUIParent != null)
        {
            originalUIScale = miniGameUIParent.transform.localScale;
            miniGameUIParent.SetActive(false);
        }

        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
        if (playerHandMop != null) playerHandMop.SetActive(false);
        if (sweetSpotMarker != null) sweetSpotMarker.enabled = false;

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;

        if (worldMop != null) mopStartPos = worldMop.transform.localPosition;
    }

    public void OnFocus()
    {
        if (miniGameActive) return;
        playerInRange = true;

        // Toggle Highlight ON
        if (highlightScript != null) highlightScript.ToggleHighlight(true);

        if (cleaningPrompt != null) cleaningPrompt.SetActive(true);
        if (cursorUI != null && interactCursorSprite != null) cursorUI.sprite = interactCursorSprite;
    }

    public void OnLoseFocus()
    {
        if (miniGameActive) return;
        playerInRange = false;

        // Toggle Highlight OFF
        if (highlightScript != null) highlightScript.ToggleHighlight(false);

        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (cursorUI != null && defaultCursorSprite != null) cursorUI.sprite = defaultCursorSprite;
    }

    public void OnInteract()
    {
        if (!miniGameActive && playerInRange)
        {
            if (worldMop != null) worldMop.SetActive(false);
            StartMiniGame();
        }
    }

    private void Update()
    {
        // 1. Pause Safety Check: Ends game if paused
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

        // Right-Click to cancel
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
        
        //cursor logic
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cursorUI != null) cursorUI.enabled = false; 

        // Turn off highlight while playing
        if (highlightScript != null) highlightScript.ToggleHighlight(false);

        if (miniGameUIParent != null)
        {
            miniGameUIParent.SetActive(true);
            miniGameUIParent.transform.localScale = originalUIScale; 
        }

        if (miniGamePrompt != null) miniGamePrompt.SetActive(true);
        if (cleaningPrompt != null) cleaningPrompt.SetActive(false);
        if (cursorUI != null) cursorUI.sprite = defaultCursorSprite;

        if (playerCam != null)
        {
            playerMovement = playerCam.GetComponentInParent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                playerAnimator = playerMovement.GetComponentInChildren<Animator>();
                
                if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
                cameraLockCoroutine = StartCoroutine(LockCameraToTarget());

                if (positioningCoroutine != null) StopCoroutine(positioningCoroutine);
                positioningCoroutine = StartCoroutine(MovePlayerToInteractPoint());
            }
        }
    }

    private void CancelMiniGame()
    {
        miniGameActive = false;
        ResetCursorState();
        
        if (playerMovement != null)
        {
            playerMovement.SyncRotation(playerCam.transform.localRotation.eulerAngles.x);
            playerMovement.enabled = true;
        }
        if (miniGameUIParent != null) miniGameUIParent.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
        if (playerHandMop != null) playerHandMop.SetActive(false);
        if (worldMop != null) worldMop.SetActive(true);
    }

    private IEnumerator MovePlayerToInteractPoint()
    {
        while (miniGameActive)
        {
            Vector3 messPos = transform.position;
            Vector3 playerPos = playerMovement.transform.position;
            Vector3 dirToPlayer = (playerPos - messPos).normalized;
            dirToPlayer.y = 0; 

            Vector3 targetPosition = messPos + (dirToPlayer * interactionDistance);
            Vector3 moveDiff = targetPosition - playerMovement.transform.position;
            
            if (moveDiff.magnitude > 0.01f)
            {
                CharacterController cc = playerMovement.GetComponent<CharacterController>();
                cc.Move(moveDiff * Time.deltaTime * positioningSpeed);
            }
            yield return null;
        }
    }

    private IEnumerator SuccessSequence()
    {
        isProcessingResult = true;
        if (fillImage != null) fillImage.color = successColor;
        miniGameUIParent.transform.localScale = originalUIScale * successPulseAmount;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("InteractionActive", true);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Broom, transform.position);
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

    private IEnumerator LockCameraToTarget()
    {
        Vector3 targetPos = transform.position + lookOffset;
    
        while (miniGameActive)
        {
            if (Time.timeScale > 0) 
            {
                // Recalculate target position just in case
                Vector3 currentTarget = transform.position + lookOffset;
                Vector3 direction = (currentTarget - playerCam.transform.position).normalized;

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);

                    // 3. Rotation (Y-Axis)
                    Quaternion bodyTarget = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
                    playerMovement.transform.rotation = Quaternion.Slerp(
                        playerMovement.transform.rotation, 
                        bodyTarget, 
                        Time.deltaTime * cameraLockSpeed
                    );

                    // 4. Rotation/camerapitch (X-Axis)
                    float targetX = lookRotation.eulerAngles.x;
                    if (targetX > 180) targetX -= 360;
                    
                    Quaternion camTarget = Quaternion.Euler(targetX, 0, 0);
                    playerCam.transform.localRotation = Quaternion.Slerp(
                        playerCam.transform.localRotation, 
                        camTarget, 
                        Time.deltaTime * cameraLockSpeed
                    );
                }
            }
            yield return null;
        }
    }

    private void FinishMiniGame()
    {
        miniGameActive = false;
        ResetCursorState();
        if (playerMovement != null)
        {
            playerMovement.SyncRotation(playerCam.transform.localRotation.eulerAngles.x);
            playerMovement.enabled = true;
        }
        if (playerAnimator != null) playerAnimator.SetBool("InteractionActive", false);
        if (playerHandMop != null) playerHandMop.SetActive(false);
        if (miniGamePrompt != null) miniGamePrompt.SetActive(false);
        if (doneVFX != null) Destroy(Instantiate(doneVFX, transform.position, Quaternion.identity), 2f);
        Destroy(gameObject);
    }
    
    private void ResetCursorState()
    {
        // Keep hardware mouse hidden (standard for FPS games)
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Locked;

        // Reactivate your custom crosshair image
        if (cursorUI != null)
        {
            cursorUI.enabled = true;
            cursorUI.sprite = defaultCursorSprite;
        }
    }
}