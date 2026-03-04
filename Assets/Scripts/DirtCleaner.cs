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
        if (cleaningPrompt != null) cleaningPrompt.SetActive(true);
        if (cursorUI != null && interactCursorSprite != null) cursorUI.sprite = interactCursorSprite;
    }

    public void OnLoseFocus()
    {
        if (miniGameActive) return;
        playerInRange = false;
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
        if (worldMop != null && worldMop.activeSelf)
        {
            worldMop.transform.Rotate(Vector3.up * mopRotationSpeed * Time.deltaTime, Space.World);
            float newY = mopStartPos.y + Mathf.Sin(Time.time * mopFloatSpeed) * mopFloatHeight;
            worldMop.transform.localPosition = new Vector3(worldMop.transform.localPosition.x, newY, worldMop.transform.localPosition.z);
        }

        if (!miniGameActive || isProcessingResult) return;

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
                
                // 1. Lock Camera
                if (cameraLockCoroutine != null) StopCoroutine(cameraLockCoroutine);
                cameraLockCoroutine = StartCoroutine(LockCameraToTarget());

                // 2. Smoothly Move Player to the ideal distance
                if (positioningCoroutine != null) StopCoroutine(positioningCoroutine);
                positioningCoroutine = StartCoroutine(MovePlayerToInteractPoint());
            }
        }
    }

    private IEnumerator MovePlayerToInteractPoint()
    {
        while (miniGameActive)
        {
            // Calculate a point on the ground at the correct distance
            Vector3 messPos = transform.position;
            Vector3 playerPos = playerMovement.transform.position;
            
            // Get direction from mess to player (so we move away from mess to the circle)
            Vector3 dirToPlayer = (playerPos - messPos).normalized;
            dirToPlayer.y = 0; // Keep movement on the horizontal plane

            Vector3 targetPosition = messPos + (dirToPlayer * interactionDistance);
            
            // Move the CharacterController smoothly
            Vector3 moveDiff = targetPosition - playerMovement.transform.position;
            if (moveDiff.magnitude > 0.01f)
            {
                // We use SimpleMove or Move to respect collisions while glidding
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
        while (miniGameActive)
        {
            Vector3 targetPos = transform.position + lookOffset;
            Vector3 direction = (targetPos - playerCam.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                playerMovement.transform.rotation = Quaternion.Slerp(playerMovement.transform.rotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), Time.deltaTime * cameraLockSpeed);
                float targetX = lookRotation.eulerAngles.x;
                if (targetX > 180) targetX -= 360;
                playerCam.transform.localRotation = Quaternion.Slerp(playerCam.transform.localRotation, Quaternion.Euler(targetX, 0, 0), Time.deltaTime * cameraLockSpeed);
            }
            yield return null;
        }
    }

    private void FinishMiniGame()
    {
        miniGameActive = false;
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
}