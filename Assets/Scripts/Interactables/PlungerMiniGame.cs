using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlungerMiniGame : MonoBehaviour, IInteractable
{
    [Header("Interaction & UI")]
    public GameObject plungerPrompt; 
    public GameObject crosshairDefault; 
    public GameObject crosshairInteract; 
    public HighlightEffectToilet highlightScript;

    [Header("Mini-Game UI Bar")]
    public GameObject barParent;    
    public Image barFill;           
    public Color startColor = Color.white;
    public Color dangerColor = Color.red;
    public float shakeAmount = 5f;

    [Header("Instructional UI")]
    public GameObject mouseTutorialObject; 
    public float tutorialDelay = 2.5f;     
    public float initialVisibilityTime = 2.0f;

    [Header("References")]
    public PlayerMovement playerMove;
    public Animator playerAnim;
    public GameObject plungerObject; 
    public Transform poopCylinder;
    public GameObject cleanWaterObject;

    [Header("Camera & Mounting")]
    public Transform targetMountPoint; 
    public Transform miniGameCamTarget;
    public float transitionDuration = 0.8f;
    public float exitBackoffDistance = 1.2f; 

    [Header("Resistance Gameplay Settings")]
    public float sensitivity = 2.0f;     
    public float upwardPressure = 0.1f; 
    public float winHoldTime = 0.3f;    
    
    [Header("Poop Settings")]
    public float idlePoopHeight = 0.01f; // The height when just looking at the toilet
    public float minPoopHeight = 0.1f;
    public float maxPoopHeight = 1.0f;
    
    private float plungeProgress = 0f;
    private float victoryTimer = 0f;
    private bool isPlaying = false;
    private bool isWon = false;
    private int layerIndex;
    private Vector3 originalPoopScale;
    private Vector2 barOriginalPos;
    private Coroutine tutorialRoutine;

    private Vector3 camSavedLocalPos;
    private Quaternion camSavedLocalRot;

    private void Awake()
    {
        if (playerAnim != null) layerIndex = playerAnim.GetLayerIndex("PlungerLayer");
        
        if (plungerPrompt) plungerPrompt.SetActive(false);
        if (plungerObject) plungerObject.SetActive(false);
        if (cleanWaterObject) cleanWaterObject.SetActive(false);
        
        if (barParent) {
            barOriginalPos = barParent.GetComponent<RectTransform>().anchoredPosition;
            barParent.SetActive(false);
        }
        
        if (mouseTutorialObject) mouseTutorialObject.SetActive(false);

        if (poopCylinder)
        {
            originalPoopScale = poopCylinder.localScale;
            // Set poop to idle height immediately
            poopCylinder.localScale = new Vector3(originalPoopScale.x, idlePoopHeight, originalPoopScale.z);
        }
    }

    private void Start()
    {
        if (barParent) barParent.SetActive(false);
        if (mouseTutorialObject) mouseTutorialObject.SetActive(false);
    }

    public void OnFocus()
    {
        if (isPlaying || isWon) return;
        if (highlightScript) highlightScript.ToggleHighlight(true);
        if (plungerPrompt) plungerPrompt.SetActive(true);
        if (crosshairDefault) crosshairDefault.SetActive(false);
        if (crosshairInteract) crosshairInteract.SetActive(true);
    }

    public void OnLoseFocus()
    {
        if (highlightScript) highlightScript.ToggleHighlight(false);
        if (isPlaying) return;
        if (plungerPrompt) plungerPrompt.SetActive(false);
        if (crosshairDefault) crosshairDefault.SetActive(true);
        if (crosshairInteract) crosshairInteract.SetActive(false);
    }

    public void OnInteract()
    {
        if (!isPlaying && !isWon) StartCoroutine(MountAndStartSequence());
    }

    private void Update()
    {
        if (!isPlaying) return;

        float mouseInputY = -Input.GetAxis("Mouse Y");
        float inputStrength = mouseInputY * sensitivity * Time.deltaTime;
        float resistance = upwardPressure * Time.deltaTime;
        
        plungeProgress = Mathf.Clamp01(plungeProgress + inputStrength - resistance);

        playerAnim.SetFloat("PlungeDepth", plungeProgress);
        
        // Gameplay scaling logic
        if (poopCylinder)
        {
            float currentHeight = Mathf.Lerp(maxPoopHeight, minPoopHeight, plungeProgress);
            poopCylinder.localScale = new Vector3(originalPoopScale.x, currentHeight, originalPoopScale.z);
        }

        UpdateUIFeedback();

        if (plungeProgress >= 0.96f)
        {
            victoryTimer += Time.deltaTime;
            if (victoryTimer >= winHoldTime) WinGame();
        }
        else victoryTimer = 0f;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            StartCoroutine(EndMiniGameSequence());
    }

    private void UpdateUIFeedback()
    {
        if (!barFill || !barParent) return;

        barFill.fillAmount = plungeProgress;
        barFill.color = Color.Lerp(startColor, dangerColor, plungeProgress);

        RectTransform rt = barParent.GetComponent<RectTransform>();
        if (plungeProgress > 0.8f)
        {
            float shake = shakeAmount * plungeProgress;
            rt.anchoredPosition = barOriginalPos + new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));
        }
        else rt.anchoredPosition = barOriginalPos;
    }

    private IEnumerator HandleTutorialUI()
    {
        if (mouseTutorialObject == null) yield break;

        mouseTutorialObject.SetActive(true);
        yield return new WaitForSeconds(initialVisibilityTime);

        while (Mathf.Abs(Input.GetAxis("Mouse Y")) < 0.2f)
        {
            yield return null;
        }

        mouseTutorialObject.SetActive(false);

        while (isPlaying)
        {
            float idleCounter = 0f;
            while (Mathf.Abs(Input.GetAxis("Mouse Y")) < 0.1f)
            {
                idleCounter += Time.deltaTime;
                if (idleCounter >= tutorialDelay)
                {
                    mouseTutorialObject.SetActive(true);
                }
                yield return null;
            }

            mouseTutorialObject.SetActive(false);
            yield return null;
        }
    }

    void WinGame()
    {
        isWon = true;
        if (poopCylinder) poopCylinder.gameObject.SetActive(false);
        if (cleanWaterObject) cleanWaterObject.SetActive(true);
        StartCoroutine(EndMiniGameSequence());
    }

    private IEnumerator MountAndStartSequence()
    {
        if (highlightScript) highlightScript.ToggleHighlight(false);
        
        isPlaying = true;
        plungeProgress = 0f; // Reset game state

        if (barParent) barParent.SetActive(true);
        if (plungerPrompt) plungerPrompt.SetActive(false);
        if (crosshairDefault) crosshairDefault.SetActive(false);
        if (crosshairInteract) crosshairInteract.SetActive(false);

        camSavedLocalPos = playerMove.playerCamera.transform.localPosition;
        camSavedLocalRot = playerMove.playerCamera.transform.localRotation;
        playerMove.enabled = false;

        float elapsed = 0;
        Vector3 startPlayerPos = playerMove.transform.position;
        Quaternion startPlayerRot = playerMove.transform.rotation;
        Vector3 startCamWorldPos = playerMove.playerCamera.transform.position;
        Quaternion startCamWorldRot = playerMove.playerCamera.transform.rotation;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);

            playerMove.transform.position = Vector3.Lerp(startPlayerPos, targetMountPoint.position, t);
            playerMove.transform.rotation = Quaternion.Slerp(startPlayerRot, targetMountPoint.rotation, t);
            playerMove.playerCamera.transform.position = Vector3.Lerp(startCamWorldPos, miniGameCamTarget.position, t);
            playerMove.playerCamera.transform.rotation = Quaternion.Slerp(startCamWorldRot, miniGameCamTarget.rotation, t);
            
            // SMOOTH POOP RISE: From idle height to game-start height
            if (poopCylinder)
            {
                float h = Mathf.Lerp(idlePoopHeight, maxPoopHeight, t);
                poopCylinder.localScale = new Vector3(originalPoopScale.x, h, originalPoopScale.z);
            }

            yield return null;
        }

        if (plungerObject) plungerObject.SetActive(true);
        playerAnim.SetTrigger("StartPlunger");
        StartCoroutine(playerMove.FadePlungerLayer(layerIndex, 1f, 0.5f));

        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);
        tutorialRoutine = StartCoroutine(HandleTutorialUI());
    }

    private IEnumerator EndMiniGameSequence()
    {
        isPlaying = false;
        
        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);
        if (mouseTutorialObject) mouseTutorialObject.SetActive(false);
        if (plungerObject) plungerObject.SetActive(false);
        if (barParent) barParent.SetActive(false);

        playerAnim.SetTrigger("StopPlunger");
        StartCoroutine(playerMove.FadePlungerLayer(layerIndex, 0f, 0.5f));

        float elapsed = 0;
        Vector3 currentCamPos = playerMove.playerCamera.transform.position;
        Quaternion currentCamRot = playerMove.playerCamera.transform.rotation;
        
        Vector3 startPlayerPos = playerMove.transform.position;
        Vector3 targetExitPos = startPlayerPos - (playerMove.transform.forward * exitBackoffDistance);

        // Capture current poop height for the reset lerp
        float currentPoopH = poopCylinder != null ? poopCylinder.localScale.y : 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            playerMove.transform.position = Vector3.Lerp(startPlayerPos, targetExitPos, t);

            Vector3 worldReturnPos = playerMove.transform.TransformPoint(camSavedLocalPos);
            Quaternion worldReturnRot = playerMove.transform.rotation * camSavedLocalRot;

            playerMove.playerCamera.transform.position = Vector3.Lerp(currentCamPos, worldReturnPos, t);
            playerMove.playerCamera.transform.rotation = Quaternion.Slerp(currentCamRot, worldReturnRot, t);

            // SMOOTH POOP RESET: If we quit/exit, grow poop back to full height
            if (poopCylinder && !isWon)
            {
                float h = Mathf.Lerp(currentPoopH, maxPoopHeight, t);
                poopCylinder.localScale = new Vector3(originalPoopScale.x, h, originalPoopScale.z);
            }

            yield return null;
        }

        playerMove.playerCamera.transform.localPosition = camSavedLocalPos;
        playerMove.playerCamera.transform.localRotation = camSavedLocalRot;
        playerMove.SyncRotation(playerMove.playerCamera.transform.localRotation.eulerAngles.x);
        playerMove.enabled = true;

        if (crosshairDefault) crosshairDefault.SetActive(true);
    }
}