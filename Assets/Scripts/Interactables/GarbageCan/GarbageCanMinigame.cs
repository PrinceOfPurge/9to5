using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GarbageCanMinigame : MonoBehaviour
{
    // Singleton instance so other scripts can easily start the mini-game
    public static GarbageCanMinigame Instance;

    // Current Can being Cleaned/Referenced
    private InteractableGarbageCan currentCan;

    // Queue representing the current sequence of keys the player must press
    private Queue<KeyCode> currentSequence = new Queue<KeyCode>();

    // List of all possible sequences. Each sequence is a List<KeyCode>.
    private List<List<KeyCode>> sequences = new List<List<KeyCode>>();

    // Tracks if the mini-game is currently active
    private bool isPlaying = false;
    
    private float activeOpacity = 1f;
    private float inactiveOpacity = 0.25f;


    public Renderer MainColor;
    
    public Image upArrowUI;
    public Image downArrowUI;
    public Image leftArrowUI;
    public Image rightArrowUI;

    void ChangeColor(Color newColor)
    {
        Material mat = MainColor.material;
        //Material mat = Instance.GetComponent<Material>();
        mat.color = newColor;
    }




    public void Start()
    {
        //MainColor = Instance.GetComponent<Renderer>();

        //Material mat = Instance.GetComponent<Material>();




    }
    
    private void Awake()
    {
        // Set up singleton instance
        Instance = this;

        

        // Add multiple sequences to the list (can be any number, order does not matter)
        sequences.Add(new List<KeyCode> { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow });
        sequences.Add(new List<KeyCode> { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow });
        sequences.Add(new List<KeyCode> { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow });
        sequences.Add(new List<KeyCode> { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow });
        // You can add as many sequences as you want here
    }

    // Call this function to start the mini-game
    public void StartMinigame(InteractableGarbageCan can)
    {
        currentCan = can;

        // Pick a random sequence from the list of sequences
        List<KeyCode> randomSeq = sequences[Random.Range(0, sequences.Count)];

        // Clear the previous sequence
        currentSequence.Clear();

        // Add the keys of the chosen sequence into the queue
        foreach (var key in randomSeq)
        {
            currentSequence.Enqueue(key);
        }

        // Debug: Show the sequence in the console so the player knows what to press
        Debug.Log("Garbage Can Mini-Game Started! Press the following sequence in order:");
        string sequenceString = "";
        foreach (var key in randomSeq)
        {
            sequenceString += key + " ";
        }
        Debug.Log(sequenceString.Trim());

        isPlaying = true;
        
        ShowNextKeyUI(currentSequence.Peek());
    }

    private void Update()
    {

        // If mini-game is not active or sequence is empty, do nothing
        if (!isPlaying || currentSequence.Count == 0)
            return;

        // Check if the player pressed the next correct key in the sequence
        if (Input.GetKeyDown(currentSequence.Peek()))
        {
            // Remove the key from the queue since it was correct
            currentSequence.Dequeue();
            Debug.Log("Correct key pressed!");

            // If queue is empty, player completed the sequence
            if (currentSequence.Count > 0)
            {
                ShowNextKeyUI(currentSequence.Peek());
            }
            else
            {
                EndMinigame();
            }
        }
        else if (Input.anyKeyDown)
        {
            // Player pressed a wrong key, restart with a new random sequence
            Debug.Log("Wrong key! Starting a new random sequence.");
            StartCoroutine(FlashWrongKey());
            Instance.StartMinigame(currentCan);
            Instance.ChangeColor(Color.red);


        }
    }

    // Call this when the mini-game is completed
    public void EndMinigame()
    {
        Debug.Log("Mini-game Completed!");
        isPlaying = false;
        Instance.ChangeColor(Color.green);
        currentCan.gameObject.SetActive(false);


        currentCan.IsCleaned = true;
        
        HideAllUI();
        
    }
    
    private void ShowNextKeyUI(KeyCode nextKey)
    {
        // ensure all arrows are enabled
        upArrowUI.gameObject.SetActive(true);
        downArrowUI.gameObject.SetActive(true);
        leftArrowUI.gameObject.SetActive(true);
        rightArrowUI.gameObject.SetActive(true);

        // Set all arrows to inactive opacity first
        SetOpacity(upArrowUI, inactiveOpacity);
        SetOpacity(downArrowUI, inactiveOpacity);
        SetOpacity(leftArrowUI, inactiveOpacity);
        SetOpacity(rightArrowUI, inactiveOpacity);

        // Now highlight the active one
        switch (nextKey)
        {
            case KeyCode.UpArrow:
                SetOpacity(upArrowUI, activeOpacity);
                break;
            case KeyCode.DownArrow:
                SetOpacity(downArrowUI, activeOpacity);
                break;
            case KeyCode.LeftArrow:
                SetOpacity(leftArrowUI, activeOpacity);
                break;
            case KeyCode.RightArrow:
                SetOpacity(rightArrowUI, activeOpacity);
                break;
        }
    }
    
    private void HideAllUI()
    {
        upArrowUI.gameObject.SetActive(false);
        downArrowUI.gameObject.SetActive(false);
        leftArrowUI.gameObject.SetActive(false);
        rightArrowUI.gameObject.SetActive(false);
    }
    
    private void SetOpacity(Image img, float opacity)
    {
        Color c = img.color;
        c.a = opacity;
        img.color = c;
    }
    
    private IEnumerator FlashWrongKey()
    {
        // Determine which UI arrow was expected
        Image expectedUI = null;

        if (currentSequence.Count > 0)
        {
            switch (currentSequence.Peek())
            {
                case KeyCode.UpArrow:
                    expectedUI = upArrowUI;
                    break;
                case KeyCode.DownArrow:
                    expectedUI = downArrowUI;
                    break;
                case KeyCode.LeftArrow:
                    expectedUI = leftArrowUI;
                    break;
                case KeyCode.RightArrow:
                    expectedUI = rightArrowUI;
                    break;
            }
        }

        if (expectedUI != null)
        {
            Color original = expectedUI.color;
            expectedUI.color = Color.red;

            yield return new WaitForSeconds(0.15f);

            expectedUI.color = original;
        }
    }
}
