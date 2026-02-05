using UnityEngine;

public class SinglePlayerStats : MonoBehaviour
{
    public int money;
    public float moveSpeed = 5f;
    public int maxHealth = 100;


    public static SinglePlayerStats Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
