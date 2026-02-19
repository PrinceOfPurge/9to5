using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject doorClosed;
    public GameObject doorOpened;
    public GameObject IntIcon;
    public float openTime = 2f;
    public static Door activeDoor;


    private bool playerInRange = false;
    private bool isOpen = false;

    void Update()
    {

        if (playerInRange && activeDoor == this && !isOpen && Input.GetKeyDown(KeyCode.C))
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        IntIcon.SetActive(false);

        doorClosed.SetActive(false);
        doorOpened.SetActive(true);

        StartCoroutine(CloseDoor());
    }

    IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(openTime);

        doorOpened.SetActive(false);
        doorClosed.SetActive(true);
        isOpen = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!playerInRange)
        {
            playerInRange = true;
            activeDoor = this;
            IntIcon.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (activeDoor == this)
        {
            playerInRange = false;
            activeDoor = null;
            IntIcon.SetActive(false);
        }
    }
}
