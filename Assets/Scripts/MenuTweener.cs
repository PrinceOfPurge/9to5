using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using DentedPixel;
using Unity.VisualScripting;
//using System.Numerics;

public class MenuTweener : MonoBehaviour
{
//--------------------Main Menu---------------------------------

//--------------------Pause Menu--------------------------------

//------------------SinglePlayer Shop---------------------------
    public GameObject UpgradeSlot1;
        public GameObject DisabledUpgradeSlot1;
        public GameObject UpgradeSlot1ConfirmBuy;
        public GameObject UpgradeSlot1TargetPos;

    public GameObject UpgradeSlot2;
        public GameObject DisabledUpgradeSlot2;
        public GameObject UpgradeSlot2ConfirmBuy;
        public GameObject UpgradeSlot2TargetPos;

    public GameObject UpgradeSlot3;
        public GameObject DisabledUpgradeSlot3;
        public GameObject UpgradeSlot3ConfirmBuy;
        public GameObject UpgradeSlot3TargetPos;

    public GameObject UpgradeSlot4;
        public GameObject DisabledUpgradeSlot4;
        public GameObject UpgradeSlot4ConfirmBuy;
        public GameObject UpgradeSlot4TargetPos;

    public GameObject ConfirmLeaveUI;
    public GameObject EquipUpgradeUI;

    public void SlideUpgradeSlotsIn()
    {
        LeanTween.move(UpgradeSlot1, UpgradeSlot1TargetPos.transform, 1.5f) .setEase( LeanTweenType.easeInOutBack );
        LeanTween.move(DisabledUpgradeSlot1, UpgradeSlot1TargetPos.transform, 1.5f) .setEase( LeanTweenType.easeInOutBack );
        
        LeanTween.move(UpgradeSlot2, UpgradeSlot2TargetPos.transform, 2.0f) .setEase( LeanTweenType.easeInOutBack );
        LeanTween.move(DisabledUpgradeSlot2, UpgradeSlot2TargetPos.transform, 2.0f) .setEase( LeanTweenType.easeInOutBack );

        LeanTween.move(UpgradeSlot3, UpgradeSlot3TargetPos.transform, 2.5f) .setEase( LeanTweenType.easeInOutBack );
        LeanTween.move(DisabledUpgradeSlot3, UpgradeSlot3TargetPos.transform, 2.5f) .setEase( LeanTweenType.easeInOutBack );
        
        LeanTween.move(UpgradeSlot4, UpgradeSlot4TargetPos.transform, 3.0f) .setEase( LeanTweenType.easeInOutBack );
        LeanTween.move(DisabledUpgradeSlot4, UpgradeSlot4TargetPos.transform, 3.0f) .setEase( LeanTweenType.easeInOutBack );

        UpgradeSlot1TargetPos.gameObject.SetActive(false);
        UpgradeSlot2TargetPos.gameObject.SetActive(false);
        UpgradeSlot3TargetPos.gameObject.SetActive(false);
        UpgradeSlot4TargetPos.gameObject.SetActive(false);
    }

    // Confirm Leave UI
    public void SlideInConfirmLeaveUI()
    {
        LeanTween.move(ConfirmLeaveUI, new Vector2(960f,550f), 2.0f) .setEase( LeanTweenType.easeOutCubic );

    }
    public void SlideOutConfirmLeaveUI()
    {
        LeanTween.move(ConfirmLeaveUI, new Vector2(960f,-600f), 0.5f) .setEase( LeanTweenType.easeInQuart );
        
    }

    // Equip Menu UI
    public void SlideInEqiupMenuUI()
    {
        LeanTween.move(EquipUpgradeUI, new Vector2(960f,550f), 2.0f) .setEase( LeanTweenType.easeOutCubic );

    }

    public void SlideOutEqiupMenuUI()
    {
        LeanTween.move(EquipUpgradeUI, new Vector2(-1000f,550f), 2.0f) .setEase( LeanTweenType.easeOutCubic );

    }

    // Confirm UI
    public void ConfirmBuySlot1()
    {
        LeanTween.move(UpgradeSlot1ConfirmBuy, new Vector2 (960f, 500f), 1.2f) .setEase( LeanTweenType.easeInOutBack );
    }
    public void DeclineBuySlot1()
    {
        LeanTween.move(UpgradeSlot1ConfirmBuy, new Vector2 (960f, 1600f), 1.4f) .setEase( LeanTweenType.easeInOutBack );
    }

    public void ConfirmBuySlot2()
    {
        LeanTween.move(UpgradeSlot2ConfirmBuy, new Vector2 (960f, 500f), 1.2f) .setEase( LeanTweenType.easeInOutBack );
    }
    public void DeclineBuySlot2()
    {
        LeanTween.move(UpgradeSlot2ConfirmBuy, new Vector2 (960f, 1600f), 1.4f) .setEase( LeanTweenType.easeInOutBack );
    }

    public void ConfirmBuySlot3()
    {
        LeanTween.move(UpgradeSlot3ConfirmBuy, new Vector2 (960f, 500f), 1.2f) .setEase( LeanTweenType.easeInOutBack );   
    }
    public void DeclineBuySlot3()
    {
        LeanTween.move(UpgradeSlot3ConfirmBuy, new Vector2 (960f, 1600f), 1.4f) .setEase( LeanTweenType.easeInOutBack );
    }

    public void ConfirmBuySlot4()
    {
        LeanTween.move(UpgradeSlot4ConfirmBuy, new Vector2 (960f, 500f), 1.2f) .setEase( LeanTweenType.easeInOutBack );
    }
    public void DeclineBuySlot4()
    {
        LeanTween.move(UpgradeSlot4ConfirmBuy, new Vector2 (960f, 1600f), 1.4f) .setEase( LeanTweenType.easeInOutBack );
    }




    



    
}
