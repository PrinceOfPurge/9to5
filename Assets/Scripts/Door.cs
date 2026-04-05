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

    // ---  AI TRACKING VARIABLES ---
    private int studentsInProximity = 0;
    private bool openedByPlayer = false; 

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
        {
            openedByPlayer = true; // The player manually claimed this door
            OpenDoor();
        }
        else 
        {
            openedByPlayer = false; // The player explicitly closed it
            CloseDoor();
        }
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

    // ---  AI Proximity Detection ---
    private void OnTriggerEnter(Collider other)
    {
        // Make sure you tag your student prefab as "Student"!
        if (other.CompareTag("Student"))
        {
            studentsInProximity++;
            
            // If the door is closed, open it for them
            if (!isOpen)
            {
                OpenDoor();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("DOOR TRIGGER HIT BY: " + other.gameObject.name + " with Tag: " + other.tag);
        if (other.CompareTag("Student"))
        {
            studentsInProximity--;
            
            // Only auto-close if no more students are near, 
            // AND the player didn't manually open this door themselves
            if (studentsInProximity <= 0 && !openedByPlayer && isOpen)
            {
                CloseDoor();
            }
        }
    }
    // -----------------------------------

    private void PlayDoorSound(bool opening)
    {
        if (AudioManager.instance == null) return;

        FMODUnity.EventReference targetEvent = opening ? FMODEvents.instance.DoorOpen : FMODEvents.instance.DoorClose;

        if (targetEvent.IsNull) return;

        // Use transform.position (the door's location) instead of the Camera's position
        AudioManager.instance.PlayOneShot(targetEvent, transform.position);
    }

    void UpdatePrompts()
    {
        if (!gameObject.activeInHierarchy) return;

        if (openPrompt) openPrompt.SetActive(isFocused && !isOpen);
        if (closePrompt) closePrompt.SetActive(isFocused && isOpen);
    }
}