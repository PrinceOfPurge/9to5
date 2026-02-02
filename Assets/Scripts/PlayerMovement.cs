using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using FMOD.Studio;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Image greenWheel;
    [SerializeField] Image redWheel;
    [SerializeField] GameObject StaminaUI;
    private Animator staminaAnimator;

    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    public Transform orientation;
    bool sprinting;
    float stamina;
    bool staminaExhausted;
    public float maxStamina;
    public float moveSpeed;
    public Camera playerCamera;

    [Header("Sensitivity Settings")]
    public float mouseSensitivity = 100f;
    private Vector2 lookInput;
    private float xRotation;

    [Header("Head Bob")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.09f;

    private float bobTimer;
    private Vector3 cameraDefaultLocalPos;

    [SerializeField] Animator playerAnimator;

    private bool offlineMode;

    Vector3 moveDirection;
    Vector2 inputDirection;

    Rigidbody rb;

    private EventInstance playerFootsteps;

    void Awake()
    {
        offlineMode = NetworkManager.Singleton == null ||
                      !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        moveSpeed = walkSpeed;
        stamina = maxStamina;

        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
        StaminaUI.SetActive(false);

        cameraDefaultLocalPos = playerCamera.transform.localPosition;

        if (mouseSensitivity <= 0f) mouseSensitivity = 100f;

        if (offlineMode)
            SetupLocalPlayer();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StaminaUI = GameObject.Find("StaminaUI");
            if (StaminaUI != null)
            {
                greenWheel = StaminaUI.transform.Find("Green Wheel").GetComponent<Image>();
                redWheel = StaminaUI.transform.Find("Red Wheel").GetComponent<Image>();
                staminaAnimator = StaminaUI.GetComponent<Animator>();
                StaminaUI.SetActive(false);
            }

            Camera sceneMainCam = Camera.main;
            if (sceneMainCam != null && sceneMainCam != playerCamera)
                sceneMainCam.enabled = false;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    private void SetupLocalPlayer()
    {
        playerCamera.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue value) => inputDirection = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        if (readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(Resetjump), jumpCooldown);
        }
    }

    public void OnSprint(InputValue value)
    {
        if (sprinting)
        {
            moveSpeed = walkSpeed;
            sprinting = false;
        }
        else
        {
            moveSpeed = sprintSpeed;
            sprinting = true;
            StaminaUI.SetActive(true);
        }
    }

    private void MovePlayer()
    {
        // ================= STAMINA (RESTORED) =================
        if (sprinting && moveDirection != Vector3.zero && grounded && !staminaExhausted)
        {
            if (stamina > 0)
            {
                stamina -= 30 * Time.deltaTime;
            }
            else
            {
                greenWheel.enabled = false;
                staminaExhausted = true;
            }

            redWheel.fillAmount = (stamina / maxStamina + 0.07f);
        }
        else
        {
            if (grounded && stamina < maxStamina)
            {
                stamina += 30 * Time.deltaTime;
            }
            else
            {
                greenWheel.enabled = true;
                staminaExhausted = false;
                if (sprinting)
                    moveSpeed = sprintSpeed;
            }

            if (stamina >= maxStamina && !sprinting && StaminaUI.activeSelf)
                StartCoroutine(PlayExitAnimation());

            redWheel.fillAmount = stamina / maxStamina;
        }

        if (staminaExhausted)
        {
            moveSpeed = walkSpeed;
            playerAnimator.SetBool("IsRunning", false);
        }

        greenWheel.fillAmount = stamina / maxStamina;
        // ======================================================

        moveDirection = orientation.forward * inputDirection.y + orientation.right * inputDirection.x;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        UpdateSound();
    }

    void Update()
    {
        if (!IsOwner && !offlineMode) return;

        HandleLook();
        HandleHeadBob();

        grounded = Physics.Raycast(transform.position, Vector3.down,
            playerHeight * 0.5f + 0.2f, whatIsGround);

        rb.drag = grounded ? groundDrag : 0f;
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        stamina -= 15f;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        redWheel.fillAmount = stamina / maxStamina + 0.07f;

        if (stamina <= 0)
        {
            greenWheel.enabled = false;
            staminaExhausted = true;
        }
    }

    private void Resetjump() => readyToJump = true;

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ================= HEAD BOB =================
    private void HandleHeadBob()
    {
        if (!grounded || inputDirection.sqrMagnitude < 0.1f)
        {
            bobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                cameraDefaultLocalPos,
                Time.deltaTime * 8f
            );
            return;
        }

        float speed = sprinting ? sprintBobSpeed : walkBobSpeed;
        float amount = sprinting ? sprintBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * speed;
        float bobOffset = Mathf.Sin(bobTimer) * amount;

        playerCamera.transform.localPosition =
            cameraDefaultLocalPos + Vector3.up * bobOffset;
    }

    private IEnumerator PlayExitAnimation()
    {
        staminaAnimator?.Play("StaminaExit");
        yield return new WaitForSeconds(0.5f);
        StaminaUI.SetActive(false);
    }

    private void UpdateSound()
    {
        if (rb.velocity.magnitude > 0.1f && grounded)
        {
            playerFootsteps.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED)
                playerFootsteps.start();
        }
        else
        {
            playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void SetMouseSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }
}
