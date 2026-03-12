using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // This allows other scripts to find it easily

    public GameObject crosshair1;
    public GameObject crosshair2;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
}
