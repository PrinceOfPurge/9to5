using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Image greenWheel;
    [SerializeField] Image redWheel;
    [SerializeField] GameObject StaminaUI;

    [Header("Stamina Pulse Settings")]
    public float pulseSpeed = 10f; 
    public float minPulseOpacity = 0.1f;
    public float maxPulseOpacity = 0.8f;
    private Color originalRedWheelColor;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -30f; 
    [Range(0, 1)] public float airMultiplier = 0.6f;
    
    [Header("Stamina Logic")]
    public float maxStamina = 100f;
    public float staminaDepletionRate = 30f;
    public float staminaRegenRate = 20f;
    public float jumpStaminaCost = 15f; 
    [Range(0, 1)] public float recoveryThreshold = 0.3f; 
    public float criticalStaminaLevel = 20f; 
    
    [Header("Detection")]
    public LayerMask whatIsGround;
    public Transform orientation; 
    public Camera playerCamera;

    [Header("Upgrades")]
    public float jumpBoostUpgradeHeight;
    public float staminaBoostUpgradeMax;
    public float rushHourUpgradeSpeedMultiplier;

    [Header("Sensitivity")]
    public float mouseSensitivity = 7f;

    [Header("Head Bob")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.09f;

    [Header("Camera Following (New)")]
    [Tooltip("Drag the 'CameraAnchor' child of your Head Bone here.")]
    public Transform cameraAnchor; 
    public float cameraFollowSpeed = 20f;
    [HideInInspector] public bool isMiniGameActive = false; // Set this to true from your Toilet script

    [SerializeField] Animator playerAnimator;

    private CharacterController controller;
    private EventInstance playerFootsteps;
    private Vector2 inputDirection;
    private Vector2 lookInput;
    private Vector3 verticalVelocity;
    private float xRotation;
    private float stamina;
    private bool staminaExhausted;
    private bool sprinting;
    private bool grounded;
    private bool readyToJump = true;
    private Vector3 cameraDefaultLocalPos;
    private float bobTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (StaminaUI != null)
        {
            StaminaUI.SetActive(true);
            if (greenWheel == null) greenWheel = StaminaUI.transform.Find("Green Wheel")?.GetComponent<Image>();
            if (redWheel == null) redWheel = StaminaUI.transform.Find("Red Wheel")?.GetComponent<Image>();
        }

        if (redWheel != null) originalRedWheelColor = redWheel.color;
        
        // This is the starting relative position of the camera
        cameraDefaultLocalPos = playerCamera.transform.localPosition;
        
        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);

        if (ShopInfo.Instance != null)
        {
            if (ShopInfo.Instance.JumpBoost_Active) jumpHeight = jumpBoostUpgradeHeight;
            if (ShopInfo.Instance.StamBoost_Active) maxStamina = staminaBoostUpgradeMax;
            if (ShopInfo.Instance.RushHour_Active)
            {
                sprintSpeed *= rushHourUpgradeSpeedMultiplier;
                walkSpeed *= rushHourUpgradeSpeedMultiplier;
            }
        }
    }

    public void OnMove(InputValue value) => inputDirection = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();
    
    public void OnJump(InputValue value) 
    { 
        if (readyToJump && grounded && !staminaExhausted && stamina >= jumpStaminaCost) 
            Jump(); 
    }
    
    public void OnSprint(InputValue value) => sprinting = value.isPressed;

    void Update()
    {
        float rayLength = controller.bounds.extents.y + 0.15f; 
        grounded = Physics.Raycast(controller.bounds.center, Vector3.down, rayLength, whatIsGround);

        if (grounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; 
        }

        HandleLook();
        HandleStamina();
        ApplyMovement();
        HandleHeadBob(); // Updated with Hybrid logic
        UpdateAnimations();
        UpdateSound();
    }

    private void HandleStamina()
    {
        bool isMoving = inputDirection.sqrMagnitude > 0.1f;

        if (sprinting && isMoving && !staminaExhausted)
        {
            stamina -= staminaDepletionRate * Time.deltaTime;
            if (stamina <= 0)
            {
                stamina = 0;
                staminaExhausted = true;
                if (greenWheel) greenWheel.enabled = false;
            }
        }
        else
        {
            if (stamina < maxStamina)
            {
                stamina += staminaRegenRate * Time.deltaTime;
                if (staminaExhausted && stamina >= (maxStamina * recoveryThreshold))
                {
                    staminaExhausted = false;
                    if (greenWheel) greenWheel.enabled = true;
                }
            }
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        bool shouldPulse = staminaExhausted || (stamina < criticalStaminaLevel);
        if (shouldPulse && redWheel != null)
        {
            float lerp = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; 
            float alpha = Mathf.Lerp(minPulseOpacity, maxPulseOpacity, lerp);
            redWheel.color = new Color(originalRedWheelColor.r, originalRedWheelColor.g, originalRedWheelColor.b, alpha);
        }
        else if (redWheel != null)
        {
            redWheel.color = originalRedWheelColor;
        }

        if (greenWheel) greenWheel.fillAmount = stamina / maxStamina;
        if (redWheel) redWheel.fillAmount = 1f; 
    }

    private void ApplyMovement()
    {
        if (isMiniGameActive) 
        {
            verticalVelocity.y += gravity * Time.deltaTime;
            if (grounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;
            controller.Move(verticalVelocity * Time.deltaTime);
            return;
        }

        Transform moveRef = orientation != null ? orientation : playerCamera.transform;
        Vector3 forward = moveRef.forward;
        Vector3 right = moveRef.right;
        forward.y = 0; right.y = 0; 

        Vector3 moveDir = (forward.normalized * inputDirection.y + right.normalized * inputDirection.x).normalized;

        float targetSpeed = (sprinting && !staminaExhausted) ? sprintSpeed : walkSpeed;
        float speed = grounded ? targetSpeed : targetSpeed * airMultiplier;
        
        Vector3 horizontalMove = moveDir * speed;

        verticalVelocity.y += gravity * Time.deltaTime;
        if (verticalVelocity.y < -50f) verticalVelocity.y = -50f; 

        Vector3 finalVelocity = horizontalMove + verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        readyToJump = false;
        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Jump");
        }

        stamina -= jumpStaminaCost;
        if (stamina < 0) stamina = 0;
        
        Invoke(nameof(ResetJump), 0.2f);
    }

    private void ResetJump() => readyToJump = true;

    private void HandleLook()
    {
        xRotation -= lookInput.y * mouseSensitivity; 
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));
    }

    private void HandleHeadBob()
    {
        // HYBRID LOGIC:
        // If we are airborne (Jumping) or in the Plunger Mini-game (Kneeling)
        // follow the BONE ANCHOR directly.
        if ((!grounded || isMiniGameActive) && cameraAnchor != null)
        {
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, cameraAnchor.position, Time.deltaTime * cameraFollowSpeed);
            return;
        }

        // WALKING/IDLE LOGIC:
        // Return to your original math for a smooth ground feel.
        if (inputDirection.sqrMagnitude < 0.1f)
        {
            bobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraDefaultLocalPos, Time.deltaTime * 8f);
            return;
        }

        float speed = (sprinting && !staminaExhausted) ? sprintBobSpeed : walkBobSpeed;
        float amount = (sprinting && !staminaExhausted) ? sprintBobAmount : walkBobAmount;
        bobTimer += Time.deltaTime * speed;
        
        // This is your original code - kept exactly the same for walking.
        playerCamera.transform.localPosition = cameraDefaultLocalPos + Vector3.up * (Mathf.Sin(bobTimer) * amount);
    }

    private void UpdateAnimations()
    {
        if (!playerAnimator) return;
        
        float animSpeed = (inputDirection.sqrMagnitude < 0.1f) ? 0f : ((sprinting && !staminaExhausted) ? 1.5f : 0.5f);
        playerAnimator.SetFloat("Speed", animSpeed);
        playerAnimator.SetBool("isGrounded", grounded);
        playerAnimator.SetFloat("VerticalVelocity", verticalVelocity.y);
    }

    private void UpdateSound()
    {
        if (controller.velocity.magnitude > 0.5f && grounded)
        {
            playerFootsteps.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED) playerFootsteps.start();
        }
        else playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
    }
    
    public IEnumerator FadePlungerLayer(int index, float target, float duration)
    {
        float startWeight = playerAnimator.GetLayerWeight(index);
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            playerAnimator.SetLayerWeight(index, Mathf.Lerp(startWeight, target, time / duration));
            yield return null;
        }
        playerAnimator.SetLayerWeight(index, target);
    }
    
    public void SyncRotation(float newXRotation)
    {
        if (newXRotation > 180) newXRotation -= 360;
        xRotation = newXRotation;
    }

    public void SetMouseSensitivity(float newSensitivity) => mouseSensitivity = newSensitivity;
}