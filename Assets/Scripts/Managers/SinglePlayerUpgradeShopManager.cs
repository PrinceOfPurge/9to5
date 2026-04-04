using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DentedPixel;
using TMPro;

public class SinglePlayerUpgradeShopManager : MonoBehaviour
{
    public GameObject ironLungsDisabled;
    public Image ironLungsUpgradeIcon;
    public Image ironLungsUpgradeIconCover;
    public Button But_IronLungsYes;
    public Button But_IronLungsNo;

    public GameObject rushhourDisabled;
    public Image rushhourUpgradeIcon;
    public Image rushhourUpgradeIconCover;
    public Button But_RushhourYes;
    public Button But_RushhourNo;

    public GameObject promotionDisabled;
    public Image promotionUpgradeIcon;
    public Image promotionUpgradeIconCover;
    public Button But_PromotionYes;
    public Button But_PromotionNo;

    public GameObject coffeebreakDisabled;
    public Image coffeebreakUpgradeIcon;
    public Image coffeebreakUpgradeIconCover;
    public Button But_CoffeebreakYes;
    public Button But_CoffeebreakNo;

    public Button IronLungs_Button;
    public int IronLungs_Cost;

    public Button RushHour_Button;
    public int RushHour_Cost;

    public Button JumpBoost_Button;
    public int JumpBoost_Cost;

    public Button StamBoost_Button;
    public int StamBoost_Cost;

    public MenuTweener menuTweener;

    [SerializeField] private TMP_Text moneyText;
    int PlayerMoney;

    void Start()
    {
        if (SinglePlayerModeManager.Instance != null)
        {
            PlayerMoney = SinglePlayerModeManager.Instance.PlayerMoney;
        }

        UpdateShop();
        menuTweener.SlideUpgradeSlotsIn();
        UpdateUpgradeSlots();
        UpdateMoneyUI();

        IronLungs_Cost = 600;
        RushHour_Cost = 800;
        JumpBoost_Cost = 1000;
        StamBoost_Cost = 1200;
    }

    void Update()
    {
        MoneyCheatCode();
        UpdateMoneyUI();
        UpdateCantAfford();
        UpdateUpgradeSlots();
    }

    void UpdateShop()
    {
        if (ShopInfo.Instance.IronLungs_Active == true)
        {
            ActivateIronLungsUpgrade();
        }
        else
        {
            DeActivateIronLungsUpgrade();
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
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayerMoney += 500;
        }
    }

    public void UpdateMoneyUI()
    {
        moneyText.text = "$" + PlayerMoney;
    }

    void UpdateUpgradeSlots()
    {
        if (ShopInfo.Instance.IronLungs_Purchased == true)
        {
            IronLungs_Button.interactable = false;
            ironLungsUpgradeIconCover.gameObject.SetActive(false);
        }
        else
        {
            IronLungs_Button.interactable = true;
            ironLungsUpgradeIconCover.gameObject.SetActive(true);
        }

        if (ShopInfo.Instance.RushHour_Purchased == true)
        {
            RushHour_Button.interactable = false;
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
        if (PlayerMoney <= IronLungs_Cost)
        {
            ironLungsDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= IronLungs_Cost)
        {
            ironLungsDisabled.gameObject.SetActive(false);
        }

        if (PlayerMoney <= RushHour_Cost)
        {
            rushhourDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= RushHour_Cost)
        {
            rushhourDisabled.gameObject.SetActive(false);
        }

        if (PlayerMoney <= JumpBoost_Cost)
        {
            promotionDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= JumpBoost_Cost)
        {
            promotionDisabled.gameObject.SetActive(false);
        }

        if (PlayerMoney <= StamBoost_Cost)
        {
            coffeebreakDisabled.gameObject.SetActive(true);
        }
        if (PlayerMoney >= StamBoost_Cost)
        {
            coffeebreakDisabled.gameObject.SetActive(false);
        }
    }

    public void PurchaceIronLungsUpgrade()
    {
        PlayerMoney -= IronLungs_Cost;
        ShopInfo.Instance.IronLungs_Purchased = true;
        menuTweener.DeclineBuySlot1();
        ActivateIronLungsUpgrade();
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

    public void ActivateIronLungsUpgrade()
    {
        ShopInfo.Instance.IronLungs_Active = true;
        ironLungsUpgradeIcon.color = Color.green;
        But_IronLungsYes.interactable = false;
        But_IronLungsNo.interactable = true;
    }

    public void DeActivateIronLungsUpgrade()
    {
        ShopInfo.Instance.IronLungs_Active = false;
        ironLungsUpgradeIcon.color = Color.red;
        But_IronLungsNo.interactable = false;
        But_IronLungsYes.interactable = true;
    }

    public void ActivateJumpBoostUpgrade()
    {
        ShopInfo.Instance.JumpBoost_Active = true;
        promotionUpgradeIcon.color = Color.green;
        But_PromotionYes.interactable = false;
        But_PromotionNo.interactable = true;
    }

    public void DeActivateJumpBoostUpgrade()
    {
        ShopInfo.Instance.JumpBoost_Active = false;
        promotionUpgradeIcon.color = Color.red;
        But_PromotionNo.interactable = false;
        But_PromotionYes.interactable = true;
    }

    public void ActivateRushhourUpgrade()
    {
        ShopInfo.Instance.RushHour_Active = true;
        rushhourUpgradeIcon.color = Color.green;
        But_RushhourYes.interactable = false;
        But_RushhourNo.interactable = true;
    }

    public void DeActivateRushhourUpgrade()
    {
        ShopInfo.Instance.RushHour_Active = false;
        rushhourUpgradeIcon.color = Color.red;
        But_RushhourNo.interactable = false;
        But_RushhourYes.interactable = true;
    }

    public void ActivateStamBoostUpgrade()
    {
        ShopInfo.Instance.StamBoost_Active = true;
        coffeebreakUpgradeIcon.color = Color.green;
        But_CoffeebreakYes.interactable = false;
        But_CoffeebreakNo.interactable = true;
    }

    public void DeActivateStamBoostUpgrade()
    {
        ShopInfo.Instance.StamBoost_Active = false;
        coffeebreakUpgradeIcon.color = Color.red;
        But_CoffeebreakNo.interactable = false;
        But_CoffeebreakYes.interactable = true;
    }
}