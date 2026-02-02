using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    public GameObject GarbagePrompt;

    public string promptMessage = "(IN RANGE) Press F to Clean [Garbage Can]";

    private void Awake()
    {
        if (GarbagePrompt != null)
            GarbagePrompt.SetActive(false);
    }

    public abstract void Interact(PlayerInteractions player);

    public virtual void ShowPrompt(bool show)
    {
        if (GarbagePrompt == null)
        {
            Debug.LogWarning("GarbagePrompt not assigned on " + gameObject.name);
            return;
        }

        GarbagePrompt.SetActive(show);

        // Force hide/show all children too
        foreach (Transform child in GarbagePrompt.transform)
            child.gameObject.SetActive(show);

        if (show)
            Debug.Log(promptMessage);
    }
}

