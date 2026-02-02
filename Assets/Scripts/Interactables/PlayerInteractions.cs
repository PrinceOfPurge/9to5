using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    // Current interactable the player is touching
    private InteractableBase currentInteractable;

    // Cursor textures
    public Texture2D normalCursor;
    public Texture2D interactCursor;

    void Start()
    {
        // Set default cursor
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    void Update()
    {
        // Press F to interact
        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        InteractableBase interact = other.GetComponent<InteractableBase>();

        if (interact != null)
        {
            currentInteractable = interact;

            // Show UI prompt
            currentInteractable.ShowPrompt(true);

            // Change cursor
            Cursor.SetCursor(interactCursor, Vector2.zero, CursorMode.Auto);
        }
    }


    void OnTriggerExit(Collider other)
    {
        InteractableBase interactable = other.GetComponent<InteractableBase>();

        if (interactable != null && interactable == currentInteractable)
        {
            // Hide UI prompt
            currentInteractable.ShowPrompt(false);

            // Reset reference
            currentInteractable = null;

            // Reset cursor
            Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
        }
    }
}



