using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DentedPixel;
using TMPro;
public class SinglePlayerUpgradeShopManager : MonoBehaviour
{
    // Singleton, stays with Player to Remeber Upgrades
    public static SinglePlayerUpgradeShopManager Instance;

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

    //-----------------------------------------------------------------

    public GameObject overtimeDisabled;
        public Image overtimeUpgradeIcon;
        public Image overtimeUpgradeIconCover;
            public Button But_OvertimeYes;
            public Button But_OvertimeNo;

    public GameObject rushhourUpgrade;
    public GameObject rushhourDisabled;
        public Image rushhourUpgradeIcon;
        public Image rushhourUpgradeIconCover;
            public Button But_RushhourYes;
            public Button But_RushhourNo;

    public GameObject promotionUpgrade;
    public GameObject promotionDisabled;
        public Image promotionUpgradeIcon;
        public Image promotionUpgradeIconCover;
            public Button But_PromotionYes;
            public Button But_PromotionNo;

    

    public GameObject coffeebreakUpgrade;
    public GameObject coffeebreakDisabled;
        public Image coffeebreakUpgradeIcon;
        public Image coffeebreakUpgradeIconCover;
            public Button But_CoffeebreakYes;
            public Button But_CoffeebreakNo;

    // Overtime
    public Button Overtime_Button;
    public int Overtime_Cost;
    public bool Overtime_Purchased;
    public bool Overtime_Active;

    // RushHour
    public int RushHour_Cost;
    public bool RushHour_Purchased;
    public bool RushHour_Active;

    // JumpBoost
    public int JumpBoost_Cost;
    public bool JumpBoost_Purchased;
    public bool JumpBoost_Active;

    // StamBoost
    public int StamBoost_Cost;
    public bool StamBoost_Purchased;
    public bool StamBoost_Active;

    public MenuTweener menuTweener;

    //Player Money
    [SerializeField] private TMP_Text moneyText;
    int PlayerMoney = SinglePlayerStats.Instance.money;

    
    // Start is called before the first frame update
    void Start()
    {
        menuTweener.SlideUpgradeSlotsIn();
        UpdateUpgradeSlots();
        UpdateMoneyUI();

        Overtime_Cost = 600;
        RushHour_Cost = 800;
        JumpBoost_Cost = 1000;
        StamBoost_Cost = 1200;

    }

    // Update is called once per frame
    void Update()
    {
        //Debug
        MoneyCheatCode();

        UpdateMoneyUI();
        UpdateCantAfford();
        UpdateUpgradeSlots();


   
        
    }
    
    void MoneyCheatCode()
    {
        //Cheat Key for Money
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayerMoney += 500;
        }
        
    }

    public void UpdateMoneyUI()
    {
        moneyText.text = "$" + PlayerMoney;   //SinglePlayerStats.Instance.money.ToString();
    }

    void UpdateUpgradeSlots()
    {
        if (Overtime_Purchased == true)
        {
            Overtime_Button.interactable = false;

            //Disable EquipOn
            overtimeUpgradeIconCover.gameObject.SetActive(false);




        }
        if (Overtime_Purchased == false)
        {
            Overtime_Button.interactable = true;


            overtimeUpgradeIconCover.gameObject.SetActive(true);
        }

        if (JumpBoost_Purchased == true)
        {
            //Disable EquipOn
            promotionUpgradeIconCover.gameObject.SetActive(false);
        }
        if (JumpBoost_Purchased == false)
        {
            promotionUpgradeIconCover.gameObject.SetActive(true);
        }

    }

    void UpdateCantAfford()
    {
        // "Can't Afford" Ovetime Logic
        if (PlayerMoney <= Overtime_Cost)
        {
            //overtimeDisabled.SetActive(true);
            overtimeDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= Overtime_Cost){
            overtimeDisabled.gameObject.SetActive(false);
        }

        // "Can't Afford" Rushhour Logic
        if (PlayerMoney <= RushHour_Cost)
        {
            //overtimeDisabled.SetActive(true);
            rushhourDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= RushHour_Cost){
            rushhourDisabled.gameObject.SetActive(false);
        }

        // "Can't Afford" Promotion Logic
        if (PlayerMoney <= JumpBoost_Cost)
        {
            //overtimeDisabled.SetActive(true);
            promotionDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= JumpBoost_Cost){
            promotionDisabled.gameObject.SetActive(false);
        }

        // "Can't Afford" Coffee Logic
        if (PlayerMoney <= StamBoost_Cost)
        {
            //overtimeDisabled.SetActive(true);
            coffeebreakDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= StamBoost_Cost){
            coffeebreakDisabled.gameObject.SetActive(false);
        }







        
    }

    public void PurchaceOvetimeUpgrade()
    {
        Overtime_Purchased = true;
        menuTweener.DeclineBuySlot1();
    }
    public void PurchaceRushhourUpgrade()
    {
        RushHour_Active = true;
    }
    public void PurchaceJumpBoostUpgrade()
    {
        JumpBoost_Purchased = true;
        menuTweener.DeclineBuySlot3();
    }
    public void PurchaceStamBoostUpgrade()
    {
        StamBoost_Active = true;
    }

    public void ActivateOvetimeUpgrade()
    {
        Overtime_Active = true;
        // Actual Function of Overtime

        // Change color of Nametag
        overtimeUpgradeIcon.color = Color.green;

        // Disable activation button
        But_OvertimeYes.interactable = false;
        But_OvertimeNo.interactable = true;
    }
    public void DeActivateOvertimeUpgrade()
    {
        Overtime_Active = false;
        // Reverse Function of Overtime

        // Change color of Nametag
        overtimeUpgradeIcon.color = Color.red;

        // Disable activation button
        But_OvertimeNo.interactable = false;
        But_OvertimeYes.interactable = true;
        
    }

    public void ActivateJumpBoostUpgrade()
    {
        JumpBoost_Active = true;
        // Change color of Nametag
        promotionUpgradeIcon.color = Color.green;
        // Disable activation button
        But_PromotionYes.interactable = false;
        But_PromotionNo.interactable = true;
    }

    public void DeActivateJumpBoostUpgrade()
    {
        JumpBoost_Active = false;
        // Change color of Nametag
        promotionUpgradeIcon.color = Color.red;
        // Disable activation button
        But_PromotionNo.interactable = false;
        But_PromotionYes.interactable = true;
    }





    public void ActivateRushhourUpgrade()
    {
        RushHour_Active = true;
    }
    public void ActivateStamBoostUpgrade()
    {
        StamBoost_Active = true;
    }






}
