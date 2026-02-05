using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DentedPixel;
using TMPro;
public class SinglePlayerUpgradeShopManager : MonoBehaviour
{
    // Singleton, stays with Player to Remeber Upgrades
    /*public static SinglePlayerUpgradeShopManager Instance;

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
    */
    //-----------------------------------------------------------------

    public GameObject overtimeDisabled;
        public Image overtimeUpgradeIcon;
        public Image overtimeUpgradeIconCover;
            public Button But_OvertimeYes;
            public Button But_OvertimeNo;

    //public GameObject rushhourUpgrade;
    public GameObject rushhourDisabled;
        public Image rushhourUpgradeIcon;
        public Image rushhourUpgradeIconCover;
            public Button But_RushhourYes;
            public Button But_RushhourNo;

    //public GameObject promotionUpgrade;
    public GameObject promotionDisabled;
        public Image promotionUpgradeIcon;
        public Image promotionUpgradeIconCover;
            public Button But_PromotionYes;
            public Button But_PromotionNo;

    

    //public GameObject coffeebreakUpgrade;
    public GameObject coffeebreakDisabled;
        public Image coffeebreakUpgradeIcon;
        public Image coffeebreakUpgradeIconCover;
            public Button But_CoffeebreakYes;
            public Button But_CoffeebreakNo;

    // Overtime
    public Button Overtime_Button;
    public int Overtime_Cost;

    // RushHour
    public Button RushHour_Button;
    public int RushHour_Cost;

    // JumpBoost
    public Button JumpBoost_Button;
    public int JumpBoost_Cost;

    // StamBoost
    public Button StamBoost_Button;
    public int StamBoost_Cost;

    public MenuTweener menuTweener;

    //Player Money
    [SerializeField] private TMP_Text moneyText;
    int PlayerMoney = SinglePlayerModeManager.Instance.PlayerMoney;

    
    // Start is called before the first frame update
    void Start()
    {
        UpdateShop();
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

    void UpdateShop()
    {
        if (ShopInfo.Instance.Overtime_Active == true)
        {
            ActivateOvetimeUpgrade();
        }
        else
        {
            DeActivateOvertimeUpgrade();
        }
        if (ShopInfo.Instance.JumpBoost_Active == true)
        {
            ActivateJumpBoostUpgrade();
        }
        else
        {
            DeActivateJumpBoostUpgrade();
        }
        if (ShopInfo.Instance.RushHour_Active == true)
        {
            ActivateRushhourUpgrade();
        }
        else
        {
            DeActivateRushhourUpgrade();
        }
        if (ShopInfo.Instance.StamBoost_Active == true)
        {
            ActivateStamBoostUpgrade();
        }
        else
        {
            DeActivateStamBoostUpgrade();
        }
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
        if (ShopInfo.Instance.Overtime_Purchased == true)
        {
            Overtime_Button.interactable = false;

            //Disable EquipOn
            overtimeUpgradeIconCover.gameObject.SetActive(false);
        }
        else
        {
            Overtime_Button.interactable = true;
            overtimeUpgradeIconCover.gameObject.SetActive(true);
        }

        if (ShopInfo.Instance.RushHour_Purchased == true)
        {
            RushHour_Button.interactable = false;
            //Disable EquipOn
            rushhourUpgradeIconCover.gameObject.SetActive(false);
        }
        else
        {
            RushHour_Button.interactable = true;
            rushhourUpgradeIconCover.gameObject.SetActive(true);
        }

        if (ShopInfo.Instance.JumpBoost_Purchased == true)
        {
            JumpBoost_Button.interactable = false;
            //Disable EquipOn
            promotionUpgradeIconCover.gameObject.SetActive(false);
        }
        if (ShopInfo.Instance.JumpBoost_Purchased == false)
        {
            JumpBoost_Button.interactable = true;
            promotionUpgradeIconCover.gameObject.SetActive(true);
        }

        if (ShopInfo.Instance.StamBoost_Purchased == true)
        {
            StamBoost_Button.interactable = false;
            //Disable EquipOn
            coffeebreakUpgradeIconCover.gameObject.SetActive(false);
        }
        else
        {
            StamBoost_Button.interactable = true;
            coffeebreakUpgradeIconCover.gameObject.SetActive(true);
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
        PlayerMoney -= Overtime_Cost;
        ShopInfo.Instance.Overtime_Purchased = true;
        menuTweener.DeclineBuySlot1();
        ActivateOvetimeUpgrade();
    }
    public void PurchaceRushhourUpgrade()
    {
        PlayerMoney -= RushHour_Cost;
        ShopInfo.Instance.RushHour_Purchased = true;
        menuTweener.DeclineBuySlot2();
        ActivateRushhourUpgrade();
    }
    public void PurchaceJumpBoostUpgrade()
    {
        PlayerMoney -= JumpBoost_Cost;
        ShopInfo.Instance.JumpBoost_Purchased = true;
        menuTweener.DeclineBuySlot3();
        ActivateJumpBoostUpgrade();
    }
    public void PurchaceStamBoostUpgrade()
    {
        PlayerMoney -= StamBoost_Cost;
        ShopInfo.Instance.StamBoost_Purchased = true;
        menuTweener.DeclineBuySlot4();
        ActivateStamBoostUpgrade();
    }

    public void ActivateOvetimeUpgrade()
    {
        ShopInfo.Instance.Overtime_Active = true;
        // Actual Function of Overtime

        // Change color of Nametag
        overtimeUpgradeIcon.color = Color.green;

        // Disable activation button
        But_OvertimeYes.interactable = false;
        But_OvertimeNo.interactable = true;
    }
    public void DeActivateOvertimeUpgrade()
    {
        ShopInfo.Instance.Overtime_Active = false;
        // Reverse Function of Overtime

        // Change color of Nametag
        overtimeUpgradeIcon.color = Color.red;

        // Disable activation button
        But_OvertimeNo.interactable = false;
        But_OvertimeYes.interactable = true;
        
    }

    public void ActivateJumpBoostUpgrade()
    {
        ShopInfo.Instance.JumpBoost_Active = true;
        // Change color of Nametag
        promotionUpgradeIcon.color = Color.green;
        // Disable activation button
        But_PromotionYes.interactable = false;
        But_PromotionNo.interactable = true;
    }
    public void DeActivateJumpBoostUpgrade()
    {
        ShopInfo.Instance.JumpBoost_Active = false;
        // Change color of Nametag
        promotionUpgradeIcon.color = Color.red;
        // Disable activation button
        But_PromotionNo.interactable = false;
        But_PromotionYes.interactable = true;
    }

    public void ActivateRushhourUpgrade()
    {
        ShopInfo.Instance.RushHour_Active = true;
        // Change color of Nametag
        rushhourUpgradeIcon.color = Color.green;
        // Disable activation button
        But_RushhourYes.interactable = false;
        But_RushhourNo.interactable = true;
    }
    public void DeActivateRushhourUpgrade()
    {
        ShopInfo.Instance.RushHour_Active = false;
        // Change color of Nametag
        rushhourUpgradeIcon.color = Color.red;
        // Disable activation button
        But_RushhourNo.interactable = false;
        But_RushhourYes.interactable = true;
    }

    public void ActivateStamBoostUpgrade()
    {
        ShopInfo.Instance.StamBoost_Active = true;
        // Change color of Nametag
        coffeebreakUpgradeIcon.color = Color.green;
        // Disable activation button
        But_CoffeebreakYes.interactable = false;
        But_CoffeebreakNo.interactable = true;
    }
    public void DeActivateStamBoostUpgrade()
    {
        ShopInfo.Instance.StamBoost_Active = false;
        // Change color of Nametag
        coffeebreakUpgradeIcon.color = Color.red;
        // Disable activation button
        But_CoffeebreakNo.interactable = false;
        But_CoffeebreakYes.interactable = true;
    }






}
