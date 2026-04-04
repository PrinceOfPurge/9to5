using UnityEngine;
using System.Collections;

public class MinigameFocusManager : MonoBehaviour
{
    public static MinigameFocusManager Instance;

    [Header("Settings")]
    public float transitionSpeed = 5f;
    public float exitDuration = 0.5f; 
    public float entryDuration = 0.25f;

    private Coroutine movementRoutine;
    private Coroutine cameraRoutine;
    private Coroutine exitRoutine;
    private Coroutine alignmentRoutine; 
    
    private PlayerMovement player;
    private Camera playerCam;

    // We store raw forward vectors instead of Quaternions to prevent Gimbal Lock corruption
    private Vector3 originalBodyForward;
    private Vector3 originalCamForward;
    private float storedXRotation;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
    }

    public void StartFocus(Transform uiTarget, Vector3 offset, float distance)
    {
        if (player == null) player = FindObjectOfType<PlayerMovement>();
        if (playerCam == null) playerCam = Camera.main;

        if (player != null)
        {
            player.isMiniGameActive = true;
            
            // Store literal vectors and raw float values instead of Quaternions
            originalBodyForward = player.transform.forward;
            originalCamForward = playerCam.transform.forward; 
            storedXRotation = player.GetCurrentXRotation();

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.Move(Vector3.zero);
        }
        
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        if (exitRoutine != null) StopCoroutine(exitRoutine);
        if (alignmentRoutine != null) StopCoroutine(alignmentRoutine);
        
        alignmentRoutine = StartCoroutine(EntryAlignmentRoutine(uiTarget, distance));
        cameraRoutine = StartCoroutine(CameraRoutine(uiTarget));
    }

    public void StopFocus()
    {
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        if (alignmentRoutine != null) StopCoroutine(alignmentRoutine);
        
        exitRoutine = StartCoroutine(SmoothExitRoutine());
    }

    private IEnumerator EntryAlignmentRoutine(Transform target, float distance)
    {
        Vector3 startPos = player.transform.position;
        
        Vector3 dirFromTarget = startPos - target.position;
        dirFromTarget.y = 0;
        dirFromTarget.Normalize();
        
        if (dirFromTarget == Vector3.zero) dirFromTarget = player.transform.forward;

        Vector3 targetPos = target.position + dirFromTarget * distance;
        targetPos.y = startPos.y; 

        CharacterController cc = player.GetComponent<CharacterController>();
        bool ccWasEnabled = cc != null && cc.enabled;
        if (ccWasEnabled) cc.enabled = false;

        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - (elapsed / entryDuration), 3f); 
            
            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        player.transform.position = targetPos;
        if (ccWasEnabled) cc.enabled = true;

        movementRoutine = StartCoroutine(MovementRoutine(target, distance));
    }

    private IEnumerator SmoothExitRoutine()
    {
        if (player == null || playerCam == null) yield break;

        player.enabled = false;
        player.isMiniGameActive = true;

        float elapsed = 0f;
        
        Vector3 currentBodyForward = player.transform.forward;
        Vector3 currentCamForward = playerCam.transform.forward;

        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / exitDuration);

            // Slerp the directional vectors, completely bypassing quaternion singularities
            Vector3 targetBodyDir = Vector3.Slerp(currentBodyForward, originalBodyForward, t);
            player.transform.rotation = Quaternion.LookRotation(targetBodyDir, Vector3.up);

            Vector3 targetCamDir = Vector3.Slerp(currentCamForward, originalCamForward, t);
            playerCam.transform.rotation = Quaternion.LookRotation(targetCamDir, player.transform.up);

            yield return null;
        }

        player.transform.rotation = Quaternion.LookRotation(originalBodyForward, Vector3.up);
        playerCam.transform.rotation = Quaternion.LookRotation(originalCamForward, Vector3.up);

        // Pass the raw original float back in, guaranteeing no math errors
        player.SyncRotation(storedXRotation);
        
        player.isMiniGameActive = false;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        player.enabled = true;
    }

    private IEnumerator MovementRoutine(Transform target, float distance)
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        while (player.isMiniGameActive)
        {
            yield return new WaitForFixedUpdate();

            Vector3 playerPos = player.transform.position;
            Vector3 targetObjPos = target.position;
            
            Vector3 dirToPlayer = (playerPos - targetObjPos).normalized;
            dirToPlayer.y = 0; 
            
            if (dirToPlayer == Vector3.zero) dirToPlayer = player.transform.forward;

            Vector3 finalPoint = targetObjPos + (dirToPlayer * distance);
            Vector3 moveDiff = finalPoint - player.transform.position;
            moveDiff.y = 0; 
            
            if (moveDiff.sqrMagnitude > 0.0001f)
            {
                if (cc != null) cc.Move(moveDiff * Time.fixedDeltaTime * transitionSpeed);
            }
        }
    }

    private IEnumerator CameraRoutine(Transform uiTarget)
    {
        while (player.isMiniGameActive)
        {
            yield return null;

            if (Time.timeScale > 0 && playerCam != null && player != null)
            {
                Vector3 focusPoint = uiTarget.position;
                
                // --- Body Rotation ---
                Vector3 bodyLookDir = focusPoint - player.transform.position;
                bodyLookDir.y = 0; 
                
                if (bodyLookDir.sqrMagnitude > 0.001f)
                {
                    Vector3 newBodyDir = Vector3.RotateTowards(player.transform.forward, bodyLookDir, Time.deltaTime * transitionSpeed, 0f);
                    player.transform.rotation = Quaternion.LookRotation(newBodyDir, Vector3.up);
                }

                // --- Camera Rotation ---
                Vector3 camLookDir = (focusPoint - playerCam.transform.position).normalized;
                
                if (camLookDir.sqrMagnitude > 0.001f)
                {
                    Vector3 newCamDir = Vector3.RotateTowards(playerCam.transform.forward, camLookDir, Time.deltaTime * transitionSpeed, 0f);
                    
                    // Constrain the 'Up' direction to the player's Up direction. 
                    // This physically stops the camera from barrel-rolling when looking straight down.
                    playerCam.transform.rotation = Quaternion.LookRotation(newCamDir, player.transform.up);
                }
            }
        }
    }
}