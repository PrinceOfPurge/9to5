using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider))]
public class DirtCleaner : MonoBehaviour
{
    [Header("Interaction")]
    public GameObject cleaningPrompt;
    public KeyCode interactKey = KeyCode.E;
    public float holdTime = 2f;
    public float fadeSpeed = 1f;
    public float perfectWindow = 0.3f;
    public GameObject doneVFX;

    [Header("UI")]
    public GameObject miniGameUIParent;
    public Image fillImage;
    public Image sweetSpotMarker;

    [Header("Cursor")]
    public Image cursorUI;
    public Sprite defaultCursorSprite;
    public Sprite interactCursorSprite;

    [Header("Flash Colors")]
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float flashDuration = 0.15f;

    private SpriteRenderer sr;
    private bool playerInRange = false;
    private bool isHolding = false;
    private bool miniGameActive = false;
    private float holdTimer = 0f;
    private float currentAlpha = 1f;
    private bool playerPressed = false;

    // Player references
    private PlayerMovement playerMovement;
    public Animator playerAnimator;
    private Camera playerCam;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAlpha = sr.color.a;
        playerCam = Camera.main;

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

        if (!miniGameActive && Input.GetKeyDown(interactKey))
        {
            StartMiniGame();
        }

        if (miniGameActive)
        {
            isHolding = Input.GetKey(interactKey);
            holdTimer += Time.deltaTime;

            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(holdTimer / holdTime);

            if (sweetSpotMarker != null)
            {
                sweetSpotMarker.enabled = true;
                float fillBarWidth = ((RectTransform)fillImage.transform).rect.width;
                float markerPos = (holdTime - perfectWindow / 2f) / holdTime * fillBarWidth;
                sweetSpotMarker.rectTransform.anchoredPosition = new Vector2(markerPos, 0f);
            }

            if (holdTimer >= holdTime && !playerPressed)
            {
                playerPressed = true;
                StartCoroutine(FlashColor(fillImage, failColor));
                holdTimer = 0f;
                fillImage.fillAmount = 0f;
            }
        }

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

        if (miniGameUIParent != null)
            miniGameUIParent.SetActive(true);

        if (fillImage != null)
            fillImage.fillAmount = 0f;

        if (playerCam != null)
        {
            playerMovement = playerCam.GetComponentInParent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                playerAnimator = playerMovement.GetComponentInChildren<Animator>();
            }
        }

        if (cleaningPrompt != null)
            cleaningPrompt.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;
    }

    private void ReleaseHold()
    {
        isHolding = false;

        float perfectStart = holdTime - perfectWindow;
        float perfectEnd = holdTime + perfectWindow;

        if (holdTimer >= perfectStart && holdTimer <= perfectEnd)
        {
            StartCoroutine(SuccessSequence());
        }
        else
        {
            StartCoroutine(FlashColor(fillImage, failColor));
            holdTimer = 0f;
            if (fillImage != null)
                fillImage.fillAmount = 0f;
        }
    }

    private IEnumerator SuccessSequence()
    {
        yield return StartCoroutine(FlashColor(fillImage, successColor));

        // ✅ Trigger mop interaction animation
        if (playerAnimator != null)
            playerAnimator.SetBool("InteractionActive", true);

        float duration = 0.5f;
        Vector3 initialScale = miniGameUIParent.transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            miniGameUIParent.transform.localScale =
                Vector3.Lerp(initialScale, targetScale, t / duration);
            yield return null;
        }

        if (miniGameUIParent != null)
            miniGameUIParent.SetActive(false);

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

        // ✅ RESET ANIMATOR BEFORE DESTROY
        if (playerAnimator != null)
            playerAnimator.SetBool("InteractionActive", false);

        if (playerMovement != null)
            playerMovement.enabled = true;

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
        if (miniGameActive) return;

        playerInRange = false;

        if (cleaningPrompt != null)
            cleaningPrompt.SetActive(false);

        if (cursorUI != null && defaultCursorSprite != null)
            cursorUI.sprite = defaultCursorSprite;
    }
}
