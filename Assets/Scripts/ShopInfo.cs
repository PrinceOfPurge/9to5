using UnityEngine;

public class ShopInfo : MonoBehaviour
{
    public static ShopInfo Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IronLungs_Purchased;
    public bool IronLungs_Active;

    public bool RushHour_Purchased;
    public bool RushHour_Active;

    public bool JumpBoost_Purchased;
    public bool JumpBoost_Active;

    public bool StamBoost_Purchased;
    public bool StamBoost_Active;
}