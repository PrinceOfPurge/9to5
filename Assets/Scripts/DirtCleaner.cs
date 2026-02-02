using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider))]
public class DirtCleaner : MonoBehaviour
{
    [Header("Interaction")]
    public GameObject cleaningPrompt;
    public KeyCode interactKey = KeyCode.E;
    public float holdTime = 2f; // Time to reach perfect release
    public float fadeSpeed = 1f; // How fast the dirt fades after perfect release
    public float perfectWindow = 0.3f; // +/- seconds for perfect timing
    public GameObject doneVFX;

    [Header("UI")]
    public GameObject miniGameUIParent; // Parent containing background, foreground, fill
    public Image fillImage;             // Only this will animate
    public Image sweetSpotMarker;       // Optional: marker showing perfect release zone

    [Header("Cursor")]
    public Image cursorUI;
    public Sprite defaultCursorSprite;
    public Sprite interactCursorSprite;

    [Header("Flash Colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float flashDuration = 0.15f;

    // Internal state
    private SpriteRenderer sr;
    private bool playerInRange = false;
    private bool isHolding = false;
    private bool miniGameActive = false;
    private float holdTimer = 0f;
    private float currentAlpha = 1f;
    private bool playerPressed = false;

    // Player movement lock
    private PlayerMovement playerMovement;
    private Camera playerCam;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAlpha = sr.color.a;
        playerCam = Camera.main;

        // Disable UI initially
        if (miniGameUIParent != null)
            miniGameUIParent.SetActive(false);

        if (cleaningPrompt != null)
            cleaningPrompt.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;

        if (fillImage != null)
            fillImage.fillAmount = 0f;

        if (sweetSpotMarker != null)
            sweetSpotMarker.enabled = false;
    }

    private void Update()
    {
        if (!playerInRange) return;

        // Start holding mini-game
        if (!miniGameActive && Input.GetKeyDown(interactKey))
        {
            StartMiniGame();
        }

        if (miniGameActive)
        {
            // Track if player is holding
            if (Input.GetKey(interactKey))
                isHolding = true;
            else
                isHolding = false;

            // Animate fill image automatically
            holdTimer += Time.deltaTime;

            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTime);

            // Sweet spot marker
            if (sweetSpotMarker != null)
            {
                sweetSpotMarker.enabled = true;
                float fillBarWidth = ((RectTransform)fillImage.transform).rect.width;
                float markerPos = (holdTime - perfectWindow / 2f) / holdTime * fillBarWidth;
                sweetSpotMarker.rectTransform.anchoredPosition = new Vector2(markerPos, 0f);
            }

            // Automatic bar full
            if (holdTimer >= holdTime && !playerPressed)
            {
                playerPressed = true;
                StartCoroutine(FlashColor(fillImage, failColor));
                holdTimer = 0f;
                fillImage.fillAmount = 0f;
            }
        }

        // Release
        if (miniGameActive && Input.GetKeyUp(interactKey))
        {
            playerPressed = true;
            ReleaseHold();
        }
    }

    private void StartMiniGame()
    {
        miniGameActive = true;
        isHolding = true;
        holdTimer = 0f;
        playerPressed = false;

        // Enable UI
        if (miniGameUIParent != null)
            miniGameUIParent.SetActive(true);

        if (fillImage != null)
            fillImage.fillAmount = 0f;

        // Lock player movement
        if (playerCam != null)
        {
            playerMovement = playerCam.GetComponentInParent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.enabled = false;
        }

        // Hide prompt
        if (cleaningPrompt != null)
            cleaningPrompt.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;
    }

    private void ReleaseHold()
    {
        isHolding = false;

        // Check timing
        float perfectStart = holdTime - perfectWindow;
        float perfectEnd = holdTime + perfectWindow;

        if (holdTimer >= perfectStart && holdTimer <= perfectEnd)
        {
            // Perfect! Shrink UI and fade dirt
            StartCoroutine(SuccessSequence());
        }
        else
        {
            // Failed attempt, flash red and reset
            StartCoroutine(FlashColor(fillImage, failColor));
            holdTimer = 0f;
            if (fillImage != null)
                fillImage.fillAmount = 0f;
        }
    }

    private IEnumerator SuccessSequence()
    {
        // Flash green on success
        yield return StartCoroutine(FlashColor(fillImage, successColor));

        // Smoothly shrink UI parent to zero
        float duration = 0.5f;
        Vector3 initialScale = miniGameUIParent.transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            miniGameUIParent.transform.localScale = Vector3.Lerp(initialScale, targetScale, lerp);
            yield return null;
        }

        if (miniGameUIParent != null)
            miniGameUIParent.SetActive(false);

        // Fade dirt sprite
        StartCoroutine(FadeDirt());
    }

    private IEnumerator FadeDirt()
    {
        while (currentAlpha > 0f)
        {
            currentAlpha -= fadeSpeed * Time.deltaTime;
            currentAlpha = Mathf.Clamp01(currentAlpha);

            Color c = sr.color;
            c.a = currentAlpha;
            sr.color = c;

            yield return null;
        }

        FinishMiniGame();
    }

    private IEnumerator FlashColor(Image img, Color color)
    {
        if (img == null) yield break;

        Color original = img.color;
        img.color = color;
        yield return new WaitForSeconds(flashDuration);
        img.color = original;
    }

    private void FinishMiniGame()
    {
        miniGameActive = false;

        // Restore player movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Play done VFX
        if (doneVFX != null)
        {
            GameObject vfx = Instantiate(doneVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;

        playerInRange = true;

        if (!miniGameActive && cleaningPrompt != null)
            cleaningPrompt.SetActive(true);

        if (cursorUI != null && interactCursorSprite != null)
            cursorUI.sprite = interactCursorSprite;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;

        if (miniGameActive) return; // lock player in

        playerInRange = false;

        if (cleaningPrompt != null)
            cleaningPrompt.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;
    }
}
