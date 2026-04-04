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

    private Quaternion originalBodyRot;
    private Quaternion originalCamRot;

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
        else Destroy(gameObject);
    }

    public void StartFocus(Transform target, Vector3 offset, float distance)
    {
        if (player == null) player = FindObjectOfType<PlayerMovement>();
        if (playerCam == null) playerCam = Camera.main;

        if (player != null)
        {
            player.isMiniGameActive = true;
            
            originalBodyRot = player.transform.rotation;
            originalCamRot = playerCam.transform.rotation; 

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.Move(Vector3.zero);
        }
        
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        if (exitRoutine != null) StopCoroutine(exitRoutine);
        if (alignmentRoutine != null) StopCoroutine(alignmentRoutine);
        
        alignmentRoutine = StartCoroutine(EntryAlignmentRoutine(target, distance));
        cameraRoutine = StartCoroutine(CameraRoutine(target, offset));
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
        Quaternion currentBodyRot = player.transform.rotation;
        Quaternion currentCamRot = playerCam.transform.rotation;

        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / exitDuration);

            player.transform.rotation = Quaternion.Slerp(currentBodyRot, originalBodyRot, t);
            playerCam.transform.rotation = Quaternion.Slerp(currentCamRot, originalCamRot, t);

            yield return null;
        }

        // Snap exactly to target
        player.transform.rotation = originalBodyRot;
        playerCam.transform.rotation = originalCamRot;

        // FIX: We no longer extract eulerAngles! Just flush the inputs and wake up.
        player.PrepareForWakeUp();
        
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

    private IEnumerator CameraRoutine(Transform target, Vector3 offset)
    {
        while (player.isMiniGameActive)
        {
            yield return null;

            if (Time.timeScale > 0 && playerCam != null && player != null)
            {
                Vector3 targetPos = target.position + offset;
                
                Vector3 bodyLookDir = targetPos - player.transform.position;
                bodyLookDir.y = 0; 
                
                if (bodyLookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetBodyRot = Quaternion.LookRotation(bodyLookDir);
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetBodyRot, Time.deltaTime * transitionSpeed);
                }

                Vector3 camLookDir = targetPos - playerCam.transform.position;
                if (camLookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetCamRot = Quaternion.LookRotation(camLookDir);
                    playerCam.transform.rotation = Quaternion.Slerp(playerCam.transform.rotation, targetCamRot, Time.deltaTime * transitionSpeed);
                }
            }
        }
    }
}