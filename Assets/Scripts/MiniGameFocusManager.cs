using UnityEngine;
using System.Collections;

public class MinigameFocusManager : MonoBehaviour
{
    public static MinigameFocusManager Instance;

    [Header("Settings")]
    public float transitionSpeed = 5f;
    
    private Coroutine movementRoutine;
    private Coroutine cameraRoutine;
    private PlayerMovement player;
    private Camera playerCam;

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
            // Stop any existing momentum
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.Move(Vector3.zero);
        }
        
        StopFocus(); // Clear any existing routines just in case
        
        movementRoutine = StartCoroutine(MovementRoutine(target, distance));
        cameraRoutine = StartCoroutine(CameraRoutine(target, offset));
    }

    public void StopFocus()
    {
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        if (cameraRoutine != null) StopCoroutine(cameraRoutine);
        
        if (player != null)
        {
            // Sync the player's look rotation so the mouse doesn't snap back
            player.SyncRotation(playerCam.transform.localEulerAngles.x);
            player.isMiniGameActive = false;
        }
    }

    // BULLETPROOF FIX 1: Physics-synced movement on a flat plane
    private IEnumerator MovementRoutine(Transform target, float distance)
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        while (player.isMiniGameActive)
        {
            yield return new WaitForFixedUpdate();

            Vector3 playerPos = player.transform.position;
            Vector3 targetObjPos = target.position;
            
            Vector3 dirToPlayer = (playerPos - targetObjPos).normalized;
            dirToPlayer.y = 0; // Keep movement on the flat plane
            
            // Failsafe so the player doesn't disappear if perfectly centered
            if (dirToPlayer == Vector3.zero) dirToPlayer = player.transform.forward;

            Vector3 finalPoint = targetObjPos + (dirToPlayer * distance);
            Vector3 moveDiff = finalPoint - player.transform.position;
            moveDiff.y = 0; // Prevent pushing the player through the floor
            
            if (moveDiff.sqrMagnitude > 0.0001f)
            {
                if (cc != null) cc.Move(moveDiff * Time.fixedDeltaTime * transitionSpeed);
            }
        }
    }

    // BULLETPROOF FIX 2: Separates Body (Y) and Camera (X) rotation to stop flipping
    private IEnumerator CameraRoutine(Transform target, Vector3 offset)
    {
        while (player.isMiniGameActive)
        {
            // Camera rotation MUST be in standard Update (yield return null) 
            // to match monitor refresh rate and prevent visual stutter.
            yield return null;

            if (Time.timeScale > 0 && playerCam != null && player != null)
            {
                Vector3 targetPos = target.position + offset;
                
                // 1. ROTATE BODY (Y Axis Only - Perfectly Flat)
                Vector3 bodyLookDir = targetPos - player.transform.position;
                bodyLookDir.y = 0; // Flatten it to prevent the Euler flipping bug
                
                if (bodyLookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetBodyRot = Quaternion.LookRotation(bodyLookDir);
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetBodyRot, Time.deltaTime * transitionSpeed);
                }

                // 2. ROTATE CAMERA (X Axis Pitch Only)
                // Use local space to calculate pitch safely
                Vector3 localTargetPos = player.transform.InverseTransformPoint(targetPos);
                Vector3 localCamDir = localTargetPos - playerCam.transform.localPosition;
                
                if (localCamDir.sqrMagnitude > 0.001f)
                {
                    Quaternion localLook = Quaternion.LookRotation(localCamDir);
                    float pitch = localLook.eulerAngles.x;
                    if (pitch > 180) pitch -= 360; 
                    
                    Quaternion targetCamRot = Quaternion.Euler(pitch, 0, 0);
                    playerCam.transform.localRotation = Quaternion.Slerp(playerCam.transform.localRotation, targetCamRot, Time.deltaTime * transitionSpeed);
                }
            }
        }
    }
}