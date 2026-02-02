using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradeBaseClass : ScriptableObject
{
    public string UpgradeName;
    public int UpgradeCost;
    public bool isPurchased;
    public bool isEquipped;

    public abstract void ApplyUpgrade(SinglePlayerStats player);   
}
