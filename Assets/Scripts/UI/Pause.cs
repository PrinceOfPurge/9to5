using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject soundMenuUI;         
    [SerializeField] private GameObject sensitivityMenuUI;  
    [SerializeField] private Slider sensitivitySlider;      // Assign your sensitivity slider here

    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool isPaused = false;
    public bool isLocalPlayer = true;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (soundMenuUI != null) soundMenuUI.SetActive(false);
        if (sensitivityMenuUI != null) sensitivityMenuUI.SetActive(false);

        // Initialize slider
        if (sensitivitySlider != null && playerMovement != null)
        {
            sensitivitySlider.minValue = 0.01f; // adjust as needed
            sensitivitySlider.maxValue = 0.2f;
            sensitivitySlider.value = playerMovement.mouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(playerMovement.SetMouseSensitivity);
        }
    }

    public void OnPause()
    {
        if (!isLocalPlayer) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        if (isPaused) return;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        foreach (var script in scriptsToDisable)
        {
            if (script != null && script != this)
                script.enabled = false;
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        isPaused = true;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (soundMenuUI != null)
            soundMenuUI.SetActive(false);

        if (sensitivityMenuUI != null)
            sensitivityMenuUI.SetActive(false);

        foreach (var script in scriptsToDisable)
        {
            if (script != null && script != this)
                script.enabled = true;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        isPaused = false;
    }

    public void ToggleSoundMenu()
    {
        if (soundMenuUI == null) return;

        bool isActive = soundMenuUI.activeSelf;
        soundMenuUI.SetActive(!isActive);

        if (!isActive && sensitivityMenuUI != null) sensitivityMenuUI.SetActive(false);
    }

    public void ToggleSensitivityMenu()
    {
        if (sensitivityMenuUI == null || playerMovement == null || sensitivitySlider == null) return;

        bool isActive = sensitivityMenuUI.activeSelf;
        sensitivityMenuUI.SetActive(!isActive);

        if (!isActive && soundMenuUI != null) soundMenuUI.SetActive(false);

        // When opening the sensitivity menu, set slider to current player sensitivity
        if (!isActive)
            sensitivitySlider.value = playerMovement.mouseSensitivity;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}
