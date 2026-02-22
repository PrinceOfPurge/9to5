using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Objects")]
    public GameObject doorClosed;
    public GameObject doorOpened;

    [Header("Prompts")]
    public GameObject openPrompt;
    public GameObject closePrompt;
    
    [Header("Crosshair UI")]
    public GameObject crosshair1; 
    public GameObject crosshair2; 

    private bool isOpen = false;
    private bool isFocused = false;

    void Start()
    {
        if (doorClosed) doorClosed.SetActive(true);
        if (doorOpened) doorOpened.SetActive(false);
        
        if (openPrompt) openPrompt.SetActive(false);
        if (closePrompt) closePrompt.SetActive(false);
        
    }

    public void OnFocus()
    {
        isFocused = true;
        if (crosshair1) crosshair1.SetActive(false);
        if (crosshair2) crosshair2.SetActive(true);
        UpdatePrompts();
    }

    public void OnLoseFocus()
    {
        isFocused = false;
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);
        UpdatePrompts();
    }

    public void OnInteract()
    {
        if (!isOpen) 
            OpenDoor();
        else 
            CloseDoor();
    }

    void OpenDoor()
    {
        isOpen = true;
        if (doorClosed) doorClosed.SetActive(false);
        if (doorOpened) doorOpened.SetActive(true);
        
        PlayDoorSound(true);

        UpdatePrompts();
    }

    void CloseDoor()
    {
        isOpen = false;
        if (doorOpened) doorOpened.SetActive(false);
        if (doorClosed) doorClosed.SetActive(true);
        
        PlayDoorSound(false);

        UpdatePrompts();
    }

    private void PlayDoorSound(bool opening)
    {
        // Check if Manager exists
        if (AudioManager.instance == null)
        {
            //Debug.LogError("DEBUG: Cannot play sound. AudioManager.instance is null.");
            return;
        }

        // 2. Select Event and Check if Reference is assigned
        FMODUnity.EventReference targetEvent = opening ? FMODEvents.instance.DoorOpen : FMODEvents.instance.DoorClose;

        if (targetEvent.IsNull)
        {
            //Debug.LogError($"DEBUG: FMOD Event '{(opening ? "DoorOpen" : "DoorClose")}' is NOT assigned in FMODEvents inspector!");
            return;
        }

        // 3. Try to play
        //Debug.Log($"DEBUG: Attempting to play {(opening ? "Open" : "Close")} sound at {transform.position}");
        AudioManager.instance.PlayOneShot(targetEvent, Camera.main.transform.position);
    }

    void UpdatePrompts()
    {
        if (!gameObject.activeInHierarchy) return;

        if (openPrompt)
            openPrompt.SetActive(isFocused && !isOpen);

        if (closePrompt)
            closePrompt.SetActive(isFocused && isOpen);
    }
}