using System.Collections;
using System.Collections.Generic;
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

        //SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public bool Overtime_Purchased;
    public bool Overtime_Active;

    // RushHour
    public bool RushHour_Purchased;
    public bool RushHour_Active;

    // JumpBoost
    public bool JumpBoost_Purchased;
    public bool JumpBoost_Active;

    // StamBoost
    public bool StamBoost_Purchased;
    public bool StamBoost_Active;

}
