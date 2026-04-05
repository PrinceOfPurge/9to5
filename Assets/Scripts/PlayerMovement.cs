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

    [Header("Upgrades (Safe Defaults)")]
    public float jumpBoostUpgradeHeight = 4f; 
    public float staminaBoostUpgradeMax = 200f; 
    public float rushHourUpgradeSpeedMultiplier = 1.35f;
    public float ironLungsRegenRate = 40f;
    [Range(0, 1)] public float ironLungsRecoveryThreshold = 0.15f;

    [Header("Sensitivity")]
    public float mouseSensitivity = 7f;

    [Header("Head Bob")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.09f;

    [Header("Camera Following")]
    public Transform cameraAnchor; 
    public float cameraFollowSpeed = 20f;
    [HideInInspector] public bool isMiniGameActive = false;

    [SerializeField] Animator playerAnimator;

    private CharacterController controller;
    private EventInstance playerFootsteps;
    
    // We bring back the EventInstance to strictly control playback
    private EventInstance outOfBreathSound;

    private Vector2 inputDirection;
    private Vector2 lookInput;
    private Vector3 verticalVelocity;
    private float xRotation;
    private float stamina;
    private bool staminaExhausted;
    private bool sprinting;
    private bool grounded;
    private bool readyToJump = true;
    private bool wasGrounded;
    private Vector3 cameraDefaultLocalPos;
    private float bobTimer;
    
    private float inputIgnoreTimer = 0f;

    void OnEnable()
    {
        lookInput = Vector2.zero;
        inputDirection = Vector2.zero;
        inputIgnoreTimer = 0.15f; 
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (StaminaUI != null)
        {
            StaminaUI.SetActive(true);
            if (greenWheel == null) greenWheel = StaminaUI.transform.Find("Green Wheel")?.GetComponent<Image>();
            if (redWheel == null) redWheel = StaminaUI.transform.Find("Red Wheel")?.GetComponent<Image>();
        }

        if (redWheel != null) originalRedWheelColor = redWheel.color;
        
        cameraDefaultLocalPos = playerCamera.transform.localPosition;
        
        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
        
        // Initialize the VO instance
        outOfBreathSound = AudioManager.instance.CreateInstance(FMODEvents.instance.OutOfBreath);

        if (ShopInfo.Instance != null)
        {
            if (ShopInfo.Instance.JumpBoost_Active) 
                jumpHeight = Mathf.Max(jumpHeight, jumpBoostUpgradeHeight);
                
            if (ShopInfo.Instance.StamBoost_Active) 
                maxStamina = Mathf.Max(maxStamina, staminaBoostUpgradeMax);
                
            if (ShopInfo.Instance.RushHour_Active)
            {
                float multi = Mathf.Max(1.1f, rushHourUpgradeSpeedMultiplier);
                sprintSpeed *= multi;
                walkSpeed *= multi;
            }

            if (ShopInfo.Instance.IronLungs_Active)
            {
                staminaRegenRate = Mathf.Max(staminaRegenRate, ironLungsRegenRate);
                recoveryThreshold = Mathf.Min(recoveryThreshold, ironLungsRecoveryThreshold);
            }
        }

        stamina = maxStamina;
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

        if (grounded && !wasGrounded && verticalVelocity.y < -5f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Land, transform.position);
        }
        wasGrounded = grounded;

        HandleLook();
        HandleStamina();
        ApplyMovement();
        HandleHeadBob();
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
                
                if (!staminaExhausted)
                {
                    staminaExhausted = true;
                    if (greenWheel) greenWheel.enabled = false;
                    
                    // CHECK PLAYBACK STATE: Only play the line if the previous one is completely finished
                    outOfBreathSound.getPlaybackState(out PLAYBACK_STATE state);
                    if (state == PLAYBACK_STATE.STOPPED)
                    {
                        outOfBreathSound.start();
                    }
                }
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
                    
                    // We intentionally DO NOT call outOfBreathSound.stop() here anymore.
                    // This allows the funny line to finish playing naturally!
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
        if (controller == null || !controller.enabled) return;

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
        
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Jump, transform.position);

        if (playerAnimator != null) playerAnimator.SetTrigger("Jump");

        stamina -= jumpStaminaCost;
        if (stamina < 0) stamina = 0;
    
        Invoke(nameof(ResetJump), 0.2f);
    }

    private void ResetJump() => readyToJump = true;

    private void HandleLook()
    {
        if (isMiniGameActive) return;
        
        if (inputIgnoreTimer > 0f)
        {
            inputIgnoreTimer -= Time.deltaTime;
            lookInput = Vector2.zero; 
        }

        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));
        
        playerCamera.transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
    }

    private void HandleHeadBob()
    {
        if ((!grounded || isMiniGameActive) && cameraAnchor != null)
        {
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, cameraAnchor.position, Time.deltaTime * cameraFollowSpeed);
            if (!isMiniGameActive)
            {
                playerCamera.transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
            }
            return;
        }
        
        if (inputDirection.sqrMagnitude < 0.1f)
        {
            bobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraDefaultLocalPos, Time.deltaTime * 8f);
            return;
        }
        
        float speed = (sprinting && !staminaExhausted) ? sprintBobSpeed : walkBobSpeed;
        float amount = (sprinting && !staminaExhausted) ? sprintBobAmount : walkBobAmount;
        bobTimer += Time.deltaTime * speed;
        
        Vector3 targetBobPos = new Vector3(cameraDefaultLocalPos.x, cameraDefaultLocalPos.y + (Mathf.Sin(bobTimer) * amount), cameraDefaultLocalPos.z);
        playerCamera.transform.localPosition = targetBobPos;
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
    
    public void PrepareForWakeUp()
    {
        lookInput = Vector2.zero; 
        inputDirection = Vector2.zero;
        inputIgnoreTimer = 0.15f; 

        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = cameraDefaultLocalPos;
        }
    }

    public void SyncRotation(float newXRotation)
    {
        if (newXRotation > 180f) newXRotation -= 360f;
        xRotation = newXRotation;
        PrepareForWakeUp(); 
    }
    
    public void SetMouseSensitivity(float newSensitivity) => mouseSensitivity = newSensitivity;
    
    void OnDestroy()
    {
        outOfBreathSound.stop(STOP_MODE.IMMEDIATE);
        outOfBreathSound.release();
    }
}