using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableGarbageCan : InteractableBase
{
    private bool isCleaned = false;
    public bool IsCleaned
    {
        get { return isCleaned; }
        set { isCleaned = value; }
    }

    

    // Inhereits Interact fuction from InteractableBase
    public override void Interact(PlayerInteractions player)
    {
        if (!isCleaned)
        {
            Debug.Log("Starting mini-game for: " + gameObject.name);

            // Starts Instance of Minigame for 1 (Instance) of Garbage Can
            GarbageCanMinigame.Instance.StartMinigame(this);
        }
        else
        {
            Debug.Log(gameObject.name + " is already cleaned.");
            //ChangeColor(Color.red);
            //gameObject.SetActive(false);
        }
    }

}
